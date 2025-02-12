using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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
using DotNetBaseQueue.RabbitMQ.Handler;
using System.Diagnostics;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using DotNetBaseQueue.Interfaces.Logs;

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
            _logger.LogInformation($"Opening channel and connection with {_queueConfiguration.HostName}.");
            var channel = ConnectionHandler.CreateConnection(_queueConfiguration, _logger);
            channel.BasicQos(prefetchSize: 0, prefetchCount: _queueConfiguration.PrefetchCount, global: false);
            _logger.LogInformation("Connection started successfully.");

            _logger.LogInformation("Starting reading messages.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var consumer = new EventingBasicConsumer(channel);

                consumer.Received += (sender, ea) => ProcessMessageAsync(channel, ea).GetAwaiter().GetResult();

                _logger.LogInformation("Starting consumer.");
                var tagConsummer = channel.BasicConsume(_queueConfiguration.QueueName, false, $"{Environment.MachineName}-{Guid.NewGuid()}", consumer: consumer);

                do
                {
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
                while (consumer.IsRunning);

                channel.BasicCancel(tagConsummer);

                _logger.LogInformation("Consumer stopped working.");
            }
        }

        private async Task ProcessMessageAsync(IModel channel, BasicDeliverEventArgs basicGetResult)
        {
            _logger.LogInformation("Message received! starting processing.");
            ValidHeader(basicGetResult);

            using var scope = _serviceProvider.CreateScope();

            var loggerScope = scope.ServiceProvider.GetService<ILogger<IEvent>>();
            var correlationIdService = scope.ServiceProvider.GetService<ICorrelationIdService>();
            var telemetryClient = scope.ServiceProvider.GetService<TelemetryClient>();

            try
            {
                if (basicGetResult.BasicProperties.Headers.TryGetValue(QueueConstraints.CORRELATION_ID_HEADER, out var correlationIdHeader))
                {
                    correlationIdService.Set(System.Text.Encoding.Default.GetString((byte[])correlationIdHeader));
                }
            }
            catch (Exception ex)
            {
                loggerScope.LogError(ex, "Error get correlation id header message.");
            }
            using var dLog = loggerScope.BeginScope(correlationIdService.Get());

            using var telemetry = telemetryClient.StartOperation<RequestTelemetry>(_queueConfiguration.QueueName);
            loggerScope.LogInformation("Starting processing message.");

            var commandHandler = scope.ServiceProvider.GetService<IEvent>();

            try
            {
                var entidade = basicGetResult.GetEntityQueue<IEntidade>(channel);

                await commandHandler.HandleAsync(entidade);

                channel.BasicAck(deliveryTag: basicGetResult.DeliveryTag, multiple: false);

                loggerScope.LogInformation("Message processed successfully.");
            }
            catch (Exception ex)
            {
                var retry = 0;
                if (!_queueConfiguration.CreateRetryQueue)
                {
                    loggerScope.LogError(ex, "Error message");
                    channel.BasicNack(basicGetResult.DeliveryTag, false, false);
                    return;
                }

                if (basicGetResult.BasicProperties.Headers.TryGetValue(RETRY_QUEUE_HEADER, out var retryString))
                {
                    _ = int.TryParse(retryString.ToString(), out retry);
                    basicGetResult.BasicProperties.Headers.Remove(RETRY_QUEUE_HEADER);
                }

                if (retry >= _queueConfiguration.NumberTryRetry)
                {
                    loggerScope.LogError(ex, "Error: num max retry.");
                    channel.BasicNack(basicGetResult.DeliveryTag, false, false);
                    return;
                }

                retry++;

                basicGetResult.BasicProperties.Headers.Add(RETRY_QUEUE_HEADER, retry);

                loggerScope.LogError(ex, "Retry message error");

                channel.BasicPublish(exchange: _queueConfiguration.ExchangeName,
                                    routingKey: $"{_queueConfiguration.QueueName}{QueueConstraints.PATH_RETRY}",
                                    basicProperties: basicGetResult.BasicProperties,
                                    body: basicGetResult.Body);

                channel.BasicAck(basicGetResult.DeliveryTag, false);
            }
            finally
            {
                telemetryClient.StopOperation(telemetry);
            }
        }

        private static void ValidHeader(BasicDeliverEventArgs basicGetResult)
        {
            if (basicGetResult.BasicProperties.Headers == null)
                basicGetResult.BasicProperties.Headers = new Dictionary<string, object>();
        }
    }
}
