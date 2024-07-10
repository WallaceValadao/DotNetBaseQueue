using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DotNetBaseQueue.Interfaces.Configs;
using DotNetBaseQueue.Interfaces;
using DotNetBaseQueue.RabbitMQ.Publicar.Interfaces;

namespace DotNetBaseQueue.RabbitMQ.Publish
{
    public class QueuePublish : IQueuePublish
    {
        private readonly IConfiguration _configuration;
        private readonly ISendMessageFactory _sendMessageFactory;

        private readonly RabbitHostConfiguration _rabbitHostConfiguration;
        private readonly ConcurrentDictionary<string, (RabbitHostConfiguration, RabbitInfoQueuePublishConfiguration)> _configurationQueues;

        public QueuePublish(IConfiguration configuration, ISendMessageFactory sendMessageFactory, RabbitHostConfiguration rabbitMQConfiguration)
        {
            _configuration = configuration;
            _sendMessageFactory = sendMessageFactory;
            _rabbitHostConfiguration = rabbitMQConfiguration;

            _configurationQueues = new ConcurrentDictionary<string, (RabbitHostConfiguration, RabbitInfoQueuePublishConfiguration)>();

            ValidateConfiguration();
        }

        private void ValidateConfiguration()
        {
            if (string.IsNullOrEmpty(_rabbitHostConfiguration.HostName))
                throw new ArgumentException("The configuration of the RabbitMQConfiguration section of the appsetings.json file is incorrect.");

            if (string.IsNullOrEmpty(_rabbitHostConfiguration.UserName))
                throw new ArgumentException("The configuration of the RabbitMQConfiguration section of the appsetings.json file is incorrect.");

            if (string.IsNullOrEmpty(_rabbitHostConfiguration.Password))
                throw new ArgumentException("The configuration of the RabbitMQConfiguration section of the appsetings.json file is incorrect.");

            if (_rabbitHostConfiguration.Port < 1)
                throw new ArgumentException("The configuration of the RabbitMQConfiguration section of the appsetings.json file is incorrect.");
        }

        private (RabbitHostConfiguration rabbitConfiguration, RabbitInfoQueuePublishConfiguration queueConfiguration) GetConfiguration(string configPublishSectionRabbitMQ)
        {
            if (!_configurationQueues.TryGetValue(configPublishSectionRabbitMQ, out var configurationQueue))
            {
                var queueConfiguration = _configuration.GetSection(configPublishSectionRabbitMQ)
                                        .Get<RabbitInfoQueuePublishConfiguration>();

                var rabbitConfiguration = _configuration.GetSection(configPublishSectionRabbitMQ).Get<RabbitHostConfiguration>();

                if (string.IsNullOrEmpty(rabbitConfiguration.HostName))
                    rabbitConfiguration = _rabbitHostConfiguration;

                configurationQueue = (rabbitConfiguration, queueConfiguration);

                _configurationQueues.TryAdd(configPublishSectionRabbitMQ, configurationQueue);
            }

            return configurationQueue;
        }

        public void Publish<T>(T entidade, string configPublishSectionRabbitMQ)
        {
            var (rabbitConfiguration, queueConfiguration) = GetConfiguration(configPublishSectionRabbitMQ);
            Publish(entidade, rabbitConfiguration, queueConfiguration.ExchangeName, queueConfiguration.RoutingKey);
        }

        public void Publish<T>(T entidade, RabbitHostConfiguration rabbitConfiguration, RabbitInfoQueuePublishConfiguration queueConfiguration)
        {
            Publish(entidade, rabbitConfiguration, queueConfiguration.ExchangeName, queueConfiguration.RoutingKey);
        }

        public void Publish<T>(T entidade, RabbitInfoQueuePublishConfiguration info)
        {
            Publish(entidade, _rabbitHostConfiguration, info.ExchangeName, info.RoutingKey);
        }

        public void Publish<T>(T entidade, string exchangeName, string routingKey)
        {
            _sendMessageFactory.Publish(_rabbitHostConfiguration, entidade, exchangeName, routingKey);
        }

        public void PublishList<T>(IEnumerable<T> entidade, string configPublishSectionRabbitMQ)
        {
            var (rabbitConfiguration, queueConfiguration) = GetConfiguration(configPublishSectionRabbitMQ);
            PublishList(entidade, rabbitConfiguration, queueConfiguration.ExchangeName, queueConfiguration.RoutingKey);
        }

        public void PublishList<T>(IEnumerable<T> entidade, RabbitHostConfiguration rabbitConfiguration, RabbitInfoQueuePublishConfiguration queueConfiguration)
        {
            PublishList(entidade, rabbitConfiguration, queueConfiguration.ExchangeName, queueConfiguration.RoutingKey);
        }

        public void PublishList<T>(IEnumerable<T> entidade, RabbitInfoQueuePublishConfiguration info)
        {
            PublishList(entidade, _rabbitHostConfiguration, info.ExchangeName, info.RoutingKey);
        }

        public void PublishList<T>(IEnumerable<T> entidade, string exchangeName, string routingKey)
        {
            PublishList<T>(entidade, _rabbitHostConfiguration, exchangeName, routingKey);
        }

        private void Publish<T>(T entidade, RabbitHostConfiguration rabbitHostConfiguration, string exchangeName, string routingKey)
        {
            _sendMessageFactory.Publish(rabbitHostConfiguration, entidade, exchangeName, routingKey);
        }

        private void PublishList<T>(IEnumerable<T> entidades, RabbitHostConfiguration rabbitHostConfiguration, string exchangeName, string routingKey)
        {
            _sendMessageFactory.PublishList(rabbitHostConfiguration, entidades, exchangeName, routingKey);
        }
    }
}
