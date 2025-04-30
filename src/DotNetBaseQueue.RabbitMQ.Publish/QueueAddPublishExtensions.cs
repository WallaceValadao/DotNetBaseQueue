using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DotNetBaseQueue.Interfaces;
using DotNetBaseQueue.Interfaces.Configs;
using DotNetBaseQueue.RabbitMQ.Publicar.Implementation;
using DotNetBaseQueue.RabbitMQ.Publicar.Interfaces;
using DotNetBaseQueue.QueueMQ.Publish;
using DotNetBaseQueue.Interfaces.Logs;
using DotNetBaseQueue.RabbitMQ.Core.Logs;
using DotNetBaseQueue.RabbitMQ.Publish.Interfaces;

namespace DotNetBaseQueue.RabbitMQ.Publish
{
    public static class QueueAddPublishExtensions
    {
        public static void AddQueuePublishSingleton(this IServiceCollection services, QueueHostConfiguration queueMQConfiguration)
        {
            services.AddQueuePublish(queueMQConfiguration);
        }

        public static void AddQueuePublishSingleton(this IServiceCollection services, IConfiguration configuration, string configSectionRabbitMQ = "QueueConfiguration")
        {
            var queueMQConfiguration = configuration.GetSection(configSectionRabbitMQ).Get<QueueHostConfiguration>();
            services.AddQueuePublish(queueMQConfiguration);
        }

        private static void AddQueuePublish(this IServiceCollection services, QueueHostConfiguration queueMQConfiguration)
        {
            services.AddSingleton<IRabbitMqConnectionFactory, RabbitMqConnectionFactory>();
            services.AddSingleton<IQueueAppConfiguration, QueueAppConfiguration>();

            services.AddSingleton(queueMQConfiguration);
            services.AddScoped<ISendMessageFactory, SendMessageFactory>();
            services.AddScoped<IQueuePublish, QueuePublish>();

            services.AddScoped<ICorrelationIdService, CorrelationIdService>();
        }
    }
}
