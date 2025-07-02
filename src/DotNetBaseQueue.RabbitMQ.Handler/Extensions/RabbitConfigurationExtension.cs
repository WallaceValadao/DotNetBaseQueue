using System.Linq;
using DotNetBaseQueue.Interfaces.Configs;

namespace DotNetBaseQueue.RabbitMQ.Handler.Extensions
{
    public static class RabbitConfigurationExtension
    {
        public static QueueConfiguration Convert(this QueueHostConfiguration queueHostConfiguration, QueueInfoQueueConfiguration queueInfoQueueConfiguration)
        {
            var queueConfig = new QueueConfiguration
            {
                HostName = queueHostConfiguration.HostName,
                Port = queueHostConfiguration.Port,
                UserName = queueHostConfiguration.UserName,
                Password = queueHostConfiguration.Password,
                VirtualHost = queueHostConfiguration.VirtualHost,
                ExchangeName = queueInfoQueueConfiguration.ExchangeName,
                ExchangeType = queueInfoQueueConfiguration.ExchangeType,
                RoutingKey = queueInfoQueueConfiguration.RoutingKey,
                QueueName = queueInfoQueueConfiguration.QueueName,
                NumberOfWorkroles = queueInfoQueueConfiguration.NumberOfWorkroles,
                CreateDeadLetterQueue = queueInfoQueueConfiguration.CreateDeadLetterQueue,
                SecondsToRetry = queueInfoQueueConfiguration.SecondsToRetry,
                CreateRetryQueue = queueInfoQueueConfiguration.CreateRetryQueue,
                RoutingKeys = queueInfoQueueConfiguration.RoutingKeys
            };

            if (!string.IsNullOrEmpty(queueConfig.QueueName))
            {
                if (string.IsNullOrEmpty(queueConfig.RoutingKey))
                    queueConfig.RoutingKey = queueConfig.QueueName;

                if (string.IsNullOrEmpty(queueConfig.ExchangeName))
                    queueConfig.ExchangeName = queueConfig.QueueName.Split('.').FirstOrDefault();
            }

            queueConfig.RoutingKeys ??= [];
            if (queueConfig.RoutingKeys.Contains(queueConfig.RoutingKey))
            {
                var routingKeys = queueConfig.RoutingKeys.ToList();
                routingKeys.Remove(queueConfig.RoutingKey);

                queueConfig.RoutingKeys = [.. routingKeys];
            }

            return queueConfig;
        }
    }
}
