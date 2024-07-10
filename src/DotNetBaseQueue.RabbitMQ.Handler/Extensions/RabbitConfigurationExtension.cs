using DotNetBaseQueue.Interfaces.Configs;

namespace DotNetBaseQueue.RabbitMQ.Handler.Extensions
{
    public static class RabbitConfigurationExtension
    {
        public static QueueConfiguration Convert(this QueueHostConfiguration queueHostConfiguration, QueueInfoQueueConfiguration queueInfoQueueConfiguration)
        {
            return new QueueConfiguration
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
            };
        }
    }
}
