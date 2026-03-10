using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTelemetry.Trace;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotNetBaseQueue.Interfaces.Configs;
using DotNetBaseQueue.Interfaces.Event;
using DotNetBaseQueue.RabbitMQ.Handler.Consumir;
using DotNetBaseQueue.RabbitMQ.Interfaces;
using DotNetBaseQueue.RabbitMQ.HostService;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using DotNetBaseQueue.RabbitMQ.Core;
using DotNetBaseQueue.Interfaces.Logs;
using DotNetBaseQueue.RabbitMQ.Handler.Telemetry;

namespace DotNetBaseQueue.QueueMQ.HostService
{
    public class RabbitConsumerHandler<IEntidade, IEvent> :
        IConsumerHandler where IEntidade : class, IQueueEvent
        where IEvent : class, IQueueEventHandler<IEntidade>
    {
        private readonly ILogger<RabbitConsumerHandler<IEntidade, IEvent>> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly QueueConfiguration _queueConfiguration;

        private readonly string _mensagemAguardando;
        private TimeSpan _secondsToRetry;

        private const string RETRY_QUEUE_HEADER = "q_retry_qt";

        public RabbitConsumerHandler(ILogger<RabbitConsumerHandler<IEntidade, IEvent>> logger, IServiceProvider serviceProvider, ConsumerConfiguration<IEntidade, IEvent> queueMQConfiguration)
        {
            _logger = logger;
            _queueConfiguration = queueMQConfiguration.QueueConfiguration;
            _serviceProvider = serviceProvider;
            _mensagemAguardando = $"({_queueConfiguration.QueueName}) Waiting for messages...";
            _secondsToRetry = TimeSpan.FromSeconds(_queueConfiguration.SecondsToRetry);
        }

        public IEnumerable<Task> CreateTask(CancellationToken stoppingToken)
        {
            foreach (var i in Enumerable.Range(0, _queueConfiguration.NumberOfWorkroles))
                yield return ProcessarAsync(stoppingToken);
        }

        private async Task ProcessarAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await QueueConsumerAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating connection.");
                    await Task.Delay(_secondsToRetry, stoppingToken);
                }
            }
        }

        private async Task QueueConsumerAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Opening channel and connection to {HostName}:{Port}",
                _queueConfiguration.HostName,
                _queueConfiguration.Port);
            var channel = await ConnectionHandler.CreateConnectionAsync(_queueConfiguration, _logger);
            await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: _queueConfiguration.PrefetchCount, global: false);
            _logger.LogInformation("Connection started successfully");

            _logger.LogInformation("Starting reading messages");

            while (!stoppingToken.IsCancellationRequested)
            {
                var consumer = new AsyncEventingBasicConsumer(channel);

                consumer.ReceivedAsync += (ch, ea) => ProcessMessageAsync(channel, ea);

                _logger.LogInformation("Starting consumer for queue {QueueName}", _queueConfiguration.QueueName);
                var tagConsummer = await channel.BasicConsumeAsync(_queueConfiguration.QueueName, false, $"{Environment.MachineName}-{Guid.NewGuid()}", consumer: consumer);

                do
                {
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
                while (consumer.IsRunning);

                await channel.BasicCancelAsync(tagConsummer);

                _logger.LogInformation("Consumer stopped working.");
            }
        }

        private async Task ProcessMessageAsync(IChannel channel, BasicDeliverEventArgs basicGetResult)
        {
            var headerMessage = GetValidHeader(basicGetResult);

            var parentContext = ExtractParentContext(headerMessage);

            using var activity = RabbitMqActivitySource.Source.StartActivity(
                $"process {_queueConfiguration.QueueName}",
                ActivityKind.Consumer,
                parentContext);

            activity?.SetTag("messaging.system", "rabbitmq");
            activity?.SetTag("messaging.destination", _queueConfiguration.QueueName);
            activity?.SetTag("messaging.destination_kind", "queue");
            activity?.SetTag("messaging.operation", "process");
            activity?.SetTag("server.address", _queueConfiguration.HostName);
            activity?.SetTag("server.port", _queueConfiguration.Port);
            activity?.SetTag("messaging.rabbitmq.routing_key", _queueConfiguration.RoutingKey);

            using var scope = _serviceProvider.CreateScope();

            var loggerScope = scope.ServiceProvider.GetService<ILogger<IEvent>>();
            var correlationIdService = scope.ServiceProvider.GetService<ICorrelationIdService>();

            try
            {
                if (headerMessage.TryGetValue(QueueConstraints.CORRELATION_ID_HEADER, out var correlationIdHeader))
                {
                    correlationIdService.Set(Encoding.Default.GetString((byte[])correlationIdHeader));
                }
            }
            catch (Exception ex)
            {
                loggerScope.LogError(ex, "Error extracting correlation id from message header");
            }

            var correlationId = correlationIdService.Get();

            activity?.SetTag("messaging.message_id", correlationId);

            using var dLog = loggerScope.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["QueueName"] = _queueConfiguration.QueueName,
                ["MachineName"] = Environment.MachineName,
                ["TraceId"] = activity?.TraceId.ToString() ?? string.Empty,
                ["SpanId"] = activity?.SpanId.ToString() ?? string.Empty
            });

            loggerScope.LogInformation("Message received, starting processing");

            var commandHandler = scope.ServiceProvider.GetService<IEvent>();

            try
            {
                var entidade = basicGetResult.GetEntityQueue<IEntidade>();

                await commandHandler.HandleAsync(entidade);

                await channel.BasicAckAsync(deliveryTag: basicGetResult.DeliveryTag, multiple: false);

                loggerScope.LogInformation("Message processed successfully");
                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            catch (Exception ex)
            {
                var retry = 0;
                if (!_queueConfiguration.CreateRetryQueue)
                {
                    loggerScope.LogError(ex, "Error processing message, retry queue is disabled");
                    activity?.AddException(ex);
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await channel.BasicNackAsync(basicGetResult.DeliveryTag, false, false);
                    return;
                }

                if (headerMessage.TryGetValue(RETRY_QUEUE_HEADER, out var retryString))
                {
                    _ = int.TryParse(retryString.ToString(), out retry);
                    headerMessage.Remove(RETRY_QUEUE_HEADER);
                }

                if (retry >= _queueConfiguration.NumberTryRetry)
                {
                    loggerScope.LogError(ex, "Message reached maximum retry attempts {MaxRetries}", _queueConfiguration.NumberTryRetry);
                    activity?.AddException(ex);
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await channel.BasicNackAsync(basicGetResult.DeliveryTag, false, false);
                    return;
                }

                retry++;
                var newProperties = MessageHelper.GetHabbitMqProperties(correlationIdService.Get(), true);
                newProperties.Headers.Add(RETRY_QUEUE_HEADER, retry);

                using var retryScope = loggerScope.BeginScope(new Dictionary<string, object>
                {
                    ["RetryCount"] = retry,
                    ["MaxRetries"] = _queueConfiguration.NumberTryRetry
                });

                loggerScope.LogError(ex, "Message processing failed, retrying {RetryCount}/{MaxRetries}", retry, _queueConfiguration.NumberTryRetry);
                activity?.SetTag("messaging.rabbitmq.retry_count", retry);
                activity?.AddException(ex);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

                await channel.BasicPublishAsync<BasicProperties>(exchange: _queueConfiguration.ExchangeName,
                                    routingKey: $"{_queueConfiguration.QueueName}{QueueConstraints.PATH_RETRY}",
                                    mandatory: true,
                                    basicProperties: newProperties,
                                    body: basicGetResult.Body);

                await channel.BasicAckAsync(basicGetResult.DeliveryTag, false);
            }
        }

        private ActivityContext ExtractParentContext(IDictionary<string, object> headers)
        {
            try
            {
                if (headers.TryGetValue("traceparent", out var traceparentRaw))
                {
                    var traceparent = Encoding.UTF8.GetString((byte[])traceparentRaw);
                    headers.TryGetValue("tracestate", out var tracestateRaw);
                    var tracestate = tracestateRaw != null
                        ? Encoding.UTF8.GetString((byte[])tracestateRaw)
                        : null;

                    if (ActivityContext.TryParse(traceparent, tracestate, out var ctx))
                        return ctx;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting parent context from message header");
            }

            return default;
        }

        private static IDictionary<string, object> GetValidHeader(BasicDeliverEventArgs basicGetResult)
        {
            return basicGetResult.BasicProperties.Headers ?? new Dictionary<string, object>();
        }
    }
}
