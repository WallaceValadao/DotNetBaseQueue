using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using DotNetBaseQueue.Interfaces.Configs;
using DotNetBaseQueue.RabbitMQ.Handler.Extensions;

namespace DotNetBaseQueue.RabbitMQ.Handler
{
    public class RabbitConsumerBuilder
    {
        internal IServiceCollection Services { get; private set; }
        internal IConfiguration Configuration { get; private set; }
        internal RabbitHostConfiguration RabbitHostConfiguration { get; private set; }

        internal RabbitConsumerBuilder(IServiceCollection services, IConfiguration configuration, string configSectionRabbitMQ)
        {
            Services = services;
            Configuration = configuration;

            AddConfiguration(configSectionRabbitMQ);
        }

        internal RabbitConsumerBuilder(IServiceCollection services, IConfiguration configuration, RabbitHostConfiguration rabbitConfiguration)
        {
            Services = services;
            Configuration = configuration;

            AddConfiguration(rabbitConfiguration);
        }

        private RabbitConsumerBuilder AddConfiguration(string configSectionRabbitMQ)
        {
            var rabbitMQConfiguration = Configuration.GetSection(configSectionRabbitMQ)
                                            .Get<RabbitHostConfiguration>();

            return AddConfiguration(rabbitMQConfiguration);
        }

        private RabbitConsumerBuilder AddConfiguration(RabbitHostConfiguration rabbitConfiguration)
        {
            RabbitHostConfiguration = rabbitConfiguration;

            return this;
        }
    }
}
