using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DotNetBaseQueue.Interfaces.Configs;

namespace DotNetBaseQueue.RabbitMQ.Handler
{
    public class RabbitConsumerBuilder
    {
        internal IServiceCollection Services { get; private set; }
        internal IConfiguration Configuration { get; private set; }
        internal QueueHostConfiguration QueueHostConfiguration { get; private set; }

        internal RabbitConsumerBuilder(IServiceCollection services, IConfiguration configuration, string configSectionQueue)
        {
            Services = services;
            Configuration = configuration;

            AddConfiguration(configSectionQueue);
        }

        internal RabbitConsumerBuilder(IServiceCollection services, IConfiguration configuration, QueueHostConfiguration queueConfiguration)
        {
            Services = services;
            Configuration = configuration;

            AddConfiguration(queueConfiguration);
        }

        private RabbitConsumerBuilder AddConfiguration(string configSectionQueue)
        {
            var rabbitMQConfiguration = Configuration.GetSection(configSectionQueue)
                                            .Get<QueueHostConfiguration>();

            return AddConfiguration(rabbitMQConfiguration);
        }

        private RabbitConsumerBuilder AddConfiguration(QueueHostConfiguration queueConfiguration)
        {
            QueueHostConfiguration = queueConfiguration;

            return this;
        }
    }
}
