using DotNetBaseQueue.Interfaces.Configs;

namespace DotNetBaseQueue.RabbitMQ.Handler.Extensions
{
    public static class RabbitConfigurationExtension
    {
        public static RabbitConfiguration Convert(this RabbitHostConfiguration rabbitMQHostConfiguration, RabbitInfoQueueConfiguration rabbitMQInfoQueueConfiguration)
        {
            return new RabbitConfiguration
            {
                HostName = rabbitMQHostConfiguration.HostName,
                Port = rabbitMQHostConfiguration.Port,
                UserName = rabbitMQHostConfiguration.UserName,
                Password = rabbitMQHostConfiguration.Password,
                VirtualHost = rabbitMQHostConfiguration.VirtualHost,
                ExchangeName = rabbitMQInfoQueueConfiguration.ExchangeName,
                ExchangeType = rabbitMQInfoQueueConfiguration.ExchangeType,
                RoutingKey = rabbitMQInfoQueueConfiguration.RoutingKey,
                QueueName = rabbitMQInfoQueueConfiguration.QueueName,
                NumberOfWorkroles = rabbitMQInfoQueueConfiguration.NumberOfWorkroles,
                CreateDeadLetterQueue = rabbitMQInfoQueueConfiguration.CreateDeadLetterQueue,
                SecondsToRetry = rabbitMQInfoQueueConfiguration.SecondsToRetry,
                CreateRetryQueue = rabbitMQInfoQueueConfiguration.CreateRetryQueue,
            };
        }
    }
}
