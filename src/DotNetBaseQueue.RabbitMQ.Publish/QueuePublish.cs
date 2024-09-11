using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DotNetBaseQueue.Interfaces.Configs;
using DotNetBaseQueue.Interfaces;
using DotNetBaseQueue.RabbitMQ.Publicar.Interfaces;

namespace DotNetBaseQueue.QueueMQ.Publish
{
    public class QueuePublish : IQueuePublish
    {
        private const string QUEUE_MQCONFIG = "The configuration of the QueueMQConfiguration section of the appsetings.json file is incorrect.";

        private readonly IConfiguration _configuration;
        private readonly ISendMessageFactory _sendMessageFactory;

        private readonly QueueHostConfiguration _queueHostConfiguration;
        private readonly ConcurrentDictionary<string, (QueueHostConfiguration, QueueInfoQueuePublishConfiguration)> _configurationQueues;

        public QueuePublish(IConfiguration configuration, ISendMessageFactory sendMessageFactory, QueueHostConfiguration QueueMQConfiguration)
        {
            _configuration = configuration;
            _sendMessageFactory = sendMessageFactory;
            _queueHostConfiguration = QueueMQConfiguration;

            _configurationQueues = new ConcurrentDictionary<string, (QueueHostConfiguration, QueueInfoQueuePublishConfiguration)>();

            ValidateConfiguration();
        }

        private void ValidateConfiguration()
        {
            if (string.IsNullOrEmpty(_queueHostConfiguration.HostName))
                throw new ArgumentException(QUEUE_MQCONFIG);

            if (string.IsNullOrEmpty(_queueHostConfiguration.UserName))
                throw new ArgumentException(QUEUE_MQCONFIG);

            if (string.IsNullOrEmpty(_queueHostConfiguration.Password))
                throw new ArgumentException(QUEUE_MQCONFIG);

            if (_queueHostConfiguration.Port < 1)
                throw new ArgumentException(QUEUE_MQCONFIG);
        }

        private (QueueHostConfiguration QueueConfiguration, QueueInfoQueuePublishConfiguration queueConfiguration) GetConfiguration(string configPublishSectionQueueMQ)
        {
            if (!_configurationQueues.TryGetValue(configPublishSectionQueueMQ, out var configurationQueue))
            {
                var queueConfiguration = _configuration.GetSection(configPublishSectionQueueMQ).Get<QueueInfoQueuePublishConfiguration>();

                var QueueConfiguration = _configuration.GetSection(configPublishSectionQueueMQ).Get<QueueHostConfiguration>();

                if (string.IsNullOrEmpty(QueueConfiguration.HostName))
                    QueueConfiguration = _queueHostConfiguration;

                configurationQueue = (QueueConfiguration, queueConfiguration);

                _configurationQueues.TryAdd(configPublishSectionQueueMQ, configurationQueue);
            }

            return configurationQueue;
        }

        public void Publish<T>(T entidade, string configPublishSectionQueueMQ)
        {
            var (QueueConfiguration, queueConfiguration) = GetConfiguration(configPublishSectionQueueMQ);
            Publish(entidade, QueueConfiguration, queueConfiguration.ExchangeName, queueConfiguration.RoutingKey);
        }

        public void Publish<T>(T entidade, QueueHostConfiguration QueueConfiguration, QueueInfoQueuePublishConfiguration queueConfiguration)
        {
            Publish(entidade, QueueConfiguration, queueConfiguration.ExchangeName, queueConfiguration.RoutingKey);
        }

        public void Publish<T>(T entidade, QueueInfoQueuePublishConfiguration info)
        {
            Publish(entidade, _queueHostConfiguration, info.ExchangeName, info.RoutingKey);
        }

        public void Publish<T>(T entidade, string exchangeName, string routingKey)
        {
            _sendMessageFactory.Publish(_queueHostConfiguration, entidade, exchangeName, routingKey);
        }

        public void PublishList<T>(IEnumerable<T> entidade, string configPublishSectionQueueMQ)
        {
            var (QueueConfiguration, queueConfiguration) = GetConfiguration(configPublishSectionQueueMQ);
            PublishList(entidade, QueueConfiguration, queueConfiguration.ExchangeName, queueConfiguration.RoutingKey);
        }

        public void PublishList<T>(IEnumerable<T> entidade, QueueHostConfiguration QueueConfiguration, QueueInfoQueuePublishConfiguration queueConfiguration)
        {
            PublishList(entidade, QueueConfiguration, queueConfiguration.ExchangeName, queueConfiguration.RoutingKey);
        }

        public void PublishList<T>(IEnumerable<T> entidade, QueueInfoQueuePublishConfiguration info)
        {
            PublishList(entidade, _queueHostConfiguration, info.ExchangeName, info.RoutingKey);
        }

        public void PublishList<T>(IEnumerable<T> entidade, string exchangeName, string routingKey)
        {
            PublishList<T>(entidade, _queueHostConfiguration, exchangeName, routingKey);
        }

        private void Publish<T>(T entidade, QueueHostConfiguration queueHostConfiguration, string exchangeName, string routingKey)
        {
            _sendMessageFactory.Publish(queueHostConfiguration, entidade, exchangeName, routingKey);
        }

        private void PublishList<T>(IEnumerable<T> entidades, QueueHostConfiguration queueHostConfiguration, string exchangeName, string routingKey)
        {
            _sendMessageFactory.PublishList(queueHostConfiguration, entidades, exchangeName, routingKey);
        }
    }
}
