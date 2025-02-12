using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DotNetBaseQueue.RabbitMQ.HostService;
using DotNetBaseQueue.Interfaces.Event;
using DotNetBaseQueue.Interfaces.Configs;
using DotNetBaseQueue.RabbitMQ.Handler.Consumir;
using DotNetBaseQueue.RabbitMQ.Interfaces;
using DotNetBaseQueue.RabbitMQ.Handler.Extensions;
using DotNetBaseQueue.QueueMQ.HostService;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DotNetBaseQueue.RabbitMQ.Core.Logs;
using DotNetBaseQueue.Interfaces.Logs;

namespace DotNetBaseQueue.RabbitMQ.Handler
{
    public static class RabbitConsumerExtensions
    {
        public static RabbitConsumerBuilder AddWorkerConfiguration(this IHostApplicationBuilder app, IConfiguration configuration, string configSectionRabbitMQ = "QueueConfiguration", bool useLogger = true)
        {
            app.AddLog(useLogger);
            return new RabbitConsumerBuilder(app.Services, configuration, configSectionRabbitMQ);
        }

        public static RabbitConsumerBuilder AddWorkerConfiguration(this IHostApplicationBuilder app, IConfiguration configuration, QueueHostConfiguration QueueConfiguration, bool useLogger = true)
        {
            app.AddLog(useLogger);
            return new RabbitConsumerBuilder(app.Services, configuration, QueueConfiguration);
        }

        private static void AddLog(this IHostApplicationBuilder app, bool useLogger)
        {
            if (!useLogger)
                return;
            app.Services.AddSingleton(typeof(ILogger<>), typeof(CorrelationIdLogger<>));
            app.Services.AddScoped<ICorrelationIdService, CorrelationIdService>();

            app.Services.AddApplicationInsightsTelemetryWorkerService();
            app.Logging.AddApplicationInsights();
        }

        public static RabbitConsumerBuilder AddHandler<IEvent, IEntity>(
            this RabbitConsumerBuilder rabbitConsumerBuilder,
            string configSectionRabbitMQ)
            where IEvent : class, IQueueEventHandler<IEntity>
            where IEntity : class, IQueueEvent
        {
            var queueConfiguration = rabbitConsumerBuilder.Configuration.GetSection(configSectionRabbitMQ).Get<QueueConfiguration>();

            if (string.IsNullOrEmpty(queueConfiguration.HostName))
            {
                queueConfiguration = rabbitConsumerBuilder.QueueHostConfiguration.Convert(queueConfiguration);
            }

            return rabbitConsumerBuilder.AddAddHandlerBase<IEvent, IEntity>(queueConfiguration);
        }

        public static RabbitConsumerBuilder AddHandler<IEvent, IEntity>(
            this RabbitConsumerBuilder rabbitConsumerBuilder,
            QueueConfiguration queueConfiguration)
            where IEvent : class, IQueueEventHandler<IEntity>
            where IEntity : class, IQueueEvent
        {
            return rabbitConsumerBuilder.AddAddHandlerBase<IEvent, IEntity>(queueConfiguration);
        }

        public static RabbitConsumerBuilder AddHandler<IEvent, IEntity>(
            this RabbitConsumerBuilder rabbitConsumerBuilder,
            string configSectionqueueHostConfiguration,
            string configSectionRabbitMQInfoQueueConfiguration)
            where IEvent : class, IQueueEventHandler<IEntity>
            where IEntity : class, IQueueEvent
        {
            var queueHostConfiguration = rabbitConsumerBuilder.Configuration.GetSection(configSectionqueueHostConfiguration).Get<QueueHostConfiguration>();

            var rabbitMQInfoQueueConfiguration = rabbitConsumerBuilder.Configuration.GetSection(configSectionRabbitMQInfoQueueConfiguration).Get<QueueInfoQueueConfiguration>();

            return rabbitConsumerBuilder.AddHandler<IEvent, IEntity>(queueHostConfiguration, rabbitMQInfoQueueConfiguration);
        }

        public static RabbitConsumerBuilder AddHandler<IEvent, IEntity>(
            this RabbitConsumerBuilder rabbitConsumerBuilder,
            QueueHostConfiguration queueHostConfiguration,
            QueueInfoQueueConfiguration rabbitMQInfoQueueConfiguration)
            where IEvent : class, IQueueEventHandler<IEntity>
            where IEntity : class, IQueueEvent
        {
            var queueConfiguration = queueHostConfiguration.Convert(rabbitMQInfoQueueConfiguration);

            return rabbitConsumerBuilder.AddAddHandlerBase<IEvent, IEntity>(queueConfiguration);
        }

        private static RabbitConsumerBuilder AddAddHandlerBase<IEvent, IEntity>(
            this RabbitConsumerBuilder rabbitConsumerBuilder,
            QueueConfiguration queueConfiguration)
            where IEvent : class, IQueueEventHandler<IEntity>
            where IEntity : class, IQueueEvent
        {
            queueConfiguration.ValidateConfig();

            rabbitConsumerBuilder.Services.AddScoped<IEvent>();
            rabbitConsumerBuilder.Services.AddSingleton(new ConsumerConfiguration<IEntity, IEvent>(queueConfiguration));
            rabbitConsumerBuilder.Services.AddSingleton<IConsumerHandler, RabbitConsumerHandler<IEntity, IEvent>>();

            rabbitConsumerBuilder.Services.AddHostedService<ConsumerHostedService>();

            return rabbitConsumerBuilder;
        }
    }
}
