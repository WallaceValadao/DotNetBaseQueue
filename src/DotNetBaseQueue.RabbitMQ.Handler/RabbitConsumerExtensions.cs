using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DotNetBaseQueue.RabbitMQ.HostService;
using System.Linq;
using DotNetBaseQueue.Interfaces.Event;
using DotNetBaseQueue.Interfaces.Configs;
using DotNetBaseQueue.RabbitMQ.Handler.Consumir;
using DotNetBaseQueue.RabbitMQ.Interfaces;
using DotNetBaseQueue.RabbitMQ.Handler.Extensions;

namespace DotNetBaseQueue.RabbitMQ.Handler
{
    public static class RabbitConsumerExtensions
    {
        public static RabbitConsumerBuilder AddWorkerConsumer(this IServiceCollection services, IConfiguration configuration, string configSectionRabbitMQ = "RabbitMQConfiguration")
        {
            return new RabbitConsumerBuilder(services, configuration, configSectionRabbitMQ);
        }

        public static RabbitConsumerBuilder AddWorkerConsumer(this IServiceCollection services, IConfiguration configuration, RabbitHostConfiguration rabbitConfiguration)
        {
            return new RabbitConsumerBuilder(services, configuration, rabbitConfiguration);
        }

        public static RabbitConsumerBuilder AddWorkerAsyncQueue<IEvent, IEntity>(
            this RabbitConsumerBuilder rabbitConsumerBuilder,
            string configSectionRabbitMQ)
            where IEvent : class, IRabbitEventHandler<IEntity>
            where IEntity : class, IRabbitEvent
        {
            var rabbitMQConfiguration = rabbitConsumerBuilder.Configuration.GetSection(configSectionRabbitMQ).Get<RabbitConfiguration>();

            if (string.IsNullOrEmpty(rabbitMQConfiguration.HostName))
            {
                rabbitMQConfiguration = rabbitConsumerBuilder.RabbitHostConfiguration.Convert(rabbitMQConfiguration);
            }

            return rabbitConsumerBuilder.AddWorkerBase<IEvent, IEntity>(rabbitMQConfiguration);
        }

        public static RabbitConsumerBuilder AddWorkerAsyncQueue<IEvent, IEntity>(
            this RabbitConsumerBuilder rabbitConsumerBuilder,
            RabbitConfiguration rabbitMQConfiguration)
            where IEvent : class, IRabbitEventHandler<IEntity>
            where IEntity : class, IRabbitEvent
        {
            return rabbitConsumerBuilder.AddWorkerBase<IEvent, IEntity>(rabbitMQConfiguration);
        }

        public static RabbitConsumerBuilder AddWorkerAsyncQueue<IEvent, IEntity>(
            this RabbitConsumerBuilder rabbitConsumerBuilder,
            string configSectionRabbitMQHostConfiguration,
            string configSectionRabbitMQInfoQueueConfiguration)
            where IEvent : class, IRabbitEventHandler<IEntity>
            where IEntity : class, IRabbitEvent
        {
            var rabbitMQHostConfiguration = rabbitConsumerBuilder.Configuration.GetSection(configSectionRabbitMQHostConfiguration).Get<RabbitHostConfiguration>();

            var rabbitMQInfoQueueConfiguration = rabbitConsumerBuilder.Configuration.GetSection(configSectionRabbitMQInfoQueueConfiguration).Get<RabbitInfoQueueConfiguration>();

            return rabbitConsumerBuilder.AddWorkerAsyncQueue<IEvent, IEntity>(rabbitMQHostConfiguration, rabbitMQInfoQueueConfiguration);
        }

        public static RabbitConsumerBuilder AddWorkerAsyncQueue<IEvent, IEntity>(
            this RabbitConsumerBuilder rabbitConsumerBuilder,
            RabbitHostConfiguration rabbitMQHostConfiguration,
            RabbitInfoQueueConfiguration rabbitMQInfoQueueConfiguration)
            where IEvent : class, IRabbitEventHandler<IEntity>
            where IEntity : class, IRabbitEvent
        {
            var rabbitMQConfiguration = rabbitMQHostConfiguration.Convert(rabbitMQInfoQueueConfiguration);

            return rabbitConsumerBuilder.AddWorkerBase<IEvent, IEntity>(rabbitMQConfiguration);
        }

        private static RabbitConsumerBuilder AddWorkerBase<IEvent, IEntity>(
            this RabbitConsumerBuilder rabbitConsumerBuilder,
            RabbitConfiguration rabbitMQConfiguration)
            where IEvent : class, IRabbitEventHandler<IEntity>
            where IEntity : class, IRabbitEvent
        {
            if (rabbitMQConfiguration.CreateRetryQueue)
            {
                var interfaces = typeof(IEntity).GetInterfaces();

                if (!interfaces.Contains(typeof(IRabbitEventRetry)))
                    SubscribeHelper.CreateException("CreateRetryQueue", "CreateRetryQueue = true and IEntity does not have the IRabbitEventRetry interface");
            }

            rabbitMQConfiguration.ValidateConfig();

            rabbitConsumerBuilder.Services.AddScoped<IEvent>();
            rabbitConsumerBuilder.Services.AddSingleton(new ConsumerConfiguration<IEntity, IEvent>(rabbitMQConfiguration));
            rabbitConsumerBuilder.Services.AddSingleton<IConsumerHandler, RabbitConsumerHandler<IEntity, IEvent>>();

            rabbitConsumerBuilder.Services.AddHostedService<ConsumerHostedService>();

            return rabbitConsumerBuilder;
        }
    }
}
