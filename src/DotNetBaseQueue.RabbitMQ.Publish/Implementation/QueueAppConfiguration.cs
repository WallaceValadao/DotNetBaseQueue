using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using DotNetBaseQueue.Interfaces.Configs;
using DotNetBaseQueue.RabbitMQ.Publish.Interfaces;

namespace DotNetBaseQueue.QueueMQ.Publish
{
    public class QueueAppConfiguration : IQueueAppConfiguration
    {
        private const string QUEUE_MQCONFIG = "The configuration of the QueueMQConfiguration section of the appsetings.json file is incorrect.";

        private readonly IConfiguration _configuration;

        private readonly QueueHostConfiguration _queueHostConfiguration;
        private readonly ConcurrentDictionary<string, (QueueHostConfiguration, QueueInfoQueuePublishConfiguration)> _configurationQueues;

        public QueueAppConfiguration(IConfiguration configuration, QueueHostConfiguration QueueMQConfiguration)
        {
            _configuration = configuration;
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

        public (QueueHostConfiguration QueueConfiguration, QueueInfoQueuePublishConfiguration queueConfiguration) GetConfiguration(string configPublishSectionQueueMQ)
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
    }
}
