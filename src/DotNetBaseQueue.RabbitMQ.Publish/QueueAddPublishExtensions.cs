using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DotNetBaseQueue.Interfaces;
using DotNetBaseQueue.Interfaces.Configs;
using DotNetBaseQueue.RabbitMQ.Publicar.Implementation;
using DotNetBaseQueue.RabbitMQ.Publicar.Interfaces;

namespace DotNetBaseQueue.RabbitMQ.Publish
{
    public static class QueueAddPublishExtensions
    {
        public static void AddQueuePublishSingleton(this IServiceCollection services, RabbitHostConfiguration rabbitMQConfiguration)
        {
            services.AddQueuePublish(rabbitMQConfiguration);
        }

        public static void AddQueuePublishSingleton(this IServiceCollection services, IConfiguration configuration, string configSectionRabbitMQ = "RabbitMQConfiguration")
        {
            var rabbitHostConfiguration = configuration.GetSection(configSectionRabbitMQ).Get<RabbitHostConfiguration>();
            services.AddQueuePublish(rabbitHostConfiguration);
        }

        private static void AddQueuePublish(this IServiceCollection services, RabbitHostConfiguration rabbitMQConfiguration)
        {
            services.AddSingleton<IRabbitMqConnectionFactory, RabbitMqConnectionFactory>();
            services.AddSingleton<ISendMessageFactory, SendMessageFactory>();

            services.AddSingleton(rabbitMQConfiguration);
            services.AddSingleton<IQueuePublish, QueuePublish>();
        }
    }
}
