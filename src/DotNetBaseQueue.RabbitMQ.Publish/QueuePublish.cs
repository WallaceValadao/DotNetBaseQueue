using System.Collections.Generic;
using DotNetBaseQueue.Interfaces.Configs;
using DotNetBaseQueue.Interfaces;
using DotNetBaseQueue.RabbitMQ.Publicar.Interfaces;
using DotNetBaseQueue.RabbitMQ.Publish.Interfaces;
using System.Threading.Tasks;

namespace DotNetBaseQueue.QueueMQ.Publish
{
    public class QueuePublish : IQueuePublish
    {
        private readonly ISendMessageFactory _sendMessageFactory;
        private readonly IQueueAppConfiguration _queueAppConfiguration;
        private readonly QueueHostConfiguration _queueHostConfiguration;

        public QueuePublish(ISendMessageFactory sendMessageFactory, IQueueAppConfiguration queueAppConfiguration, QueueHostConfiguration queueMQConfiguration)
        {
            _sendMessageFactory = sendMessageFactory;
            _queueAppConfiguration = queueAppConfiguration;
            _queueHostConfiguration = queueMQConfiguration;
        }

        public Task PublishAsync<T>(T entidade, string configPublishSectionQueueMQ)
        {
            var (QueueConfiguration, queueConfiguration) = _queueAppConfiguration.GetConfiguration(configPublishSectionQueueMQ);
            return PublishAsync(entidade, QueueConfiguration, queueConfiguration.ExchangeName, queueConfiguration.RoutingKey);
        }

        public Task PublishAsync<T>(T entidade, QueueHostConfiguration QueueConfiguration, QueueInfoQueuePublishConfiguration queueConfiguration)
        {
            return PublishAsync(entidade, QueueConfiguration, queueConfiguration.ExchangeName, queueConfiguration.RoutingKey);
        }

        public Task PublishAsync<T>(T entidade, QueueInfoQueuePublishConfiguration info)
        {
            return PublishAsync(entidade, _queueHostConfiguration, info.ExchangeName, info.RoutingKey);
        }

        public Task PublishAsync<T>(T entidade, string exchangeName, string routingKey)
        {
            return _sendMessageFactory.PublishAsync(_queueHostConfiguration, entidade, exchangeName, routingKey);
        }

        public Task PublishListAsync<T>(IEnumerable<T> entidade, string configPublishSectionQueueMQ)
        {
            var (QueueConfiguration, queueConfiguration) = _queueAppConfiguration.GetConfiguration(configPublishSectionQueueMQ);
            return PublishList(entidade, QueueConfiguration, queueConfiguration.ExchangeName, queueConfiguration.RoutingKey);
        }

        public Task PublishListAsync<T>(IEnumerable<T> entidade, QueueHostConfiguration QueueConfiguration, QueueInfoQueuePublishConfiguration queueConfiguration)
        {
            return PublishList(entidade, QueueConfiguration, queueConfiguration.ExchangeName, queueConfiguration.RoutingKey);
        }

        public Task PublishListAsync<T>(IEnumerable<T> entidade, QueueInfoQueuePublishConfiguration info)
        {
            return PublishList(entidade, _queueHostConfiguration, info.ExchangeName, info.RoutingKey);
        }

        public Task PublishListAsync<T>(IEnumerable<T> entidade, string exchangeName, string routingKey)
        {
            return PublishList<T>(entidade, _queueHostConfiguration, exchangeName, routingKey);
        }

        private Task PublishAsync<T>(T entidade, QueueHostConfiguration queueHostConfiguration, string exchangeName, string routingKey)
        {
            return _sendMessageFactory.PublishAsync(queueHostConfiguration, entidade, exchangeName, routingKey);
        }

        private Task PublishList<T>(IEnumerable<T> entidades, QueueHostConfiguration queueHostConfiguration, string exchangeName, string routingKey)
        {
            return _sendMessageFactory.PublishListAsync(queueHostConfiguration, entidades, exchangeName, routingKey);
        }
    }
}
