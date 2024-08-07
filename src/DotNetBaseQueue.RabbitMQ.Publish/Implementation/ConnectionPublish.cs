﻿using RabbitMQ.Client;
using DotNetBaseQueue.Interfaces.Configs;
using DotNetBaseQueue.RabbitMQ.Core;
using DotNetBaseQueue.RabbitMQ.Publicar.Interfaces;

namespace DotNetBaseQueue.RabbitMQ.Publicar.Implementation
{
    public class ConnectionPublish : IConnectionPublish
    {
        public readonly ConnectionFactory connectionFactory;
        public readonly IConnection connection;
        public IModel Channel { get; private set; }

        internal ConnectionPublish(QueueHostConfiguration queueHostConfiguration)
        {
            connectionFactory = new ConnectionFactory()
            {
                HostName = queueHostConfiguration.HostName,
                Port = queueHostConfiguration.Port,
                UserName = queueHostConfiguration.UserName,
                Password = queueHostConfiguration.Password,
                VirtualHost = queueHostConfiguration.VirtualHost
            };

            connection = connectionFactory.CreateConnection();
            Channel = connection.CreateModel();
        }

        public void Dispose()
        {
            Channel.Close();
            connection.Close();
        }

        public void Publish(string exchangeName, string routingKey, bool mandatory, byte[] body)
        {
            var properties = Channel.GetHabbitMqProperties();

            Channel.BasicPublish(exchangeName, routingKey, mandatory, properties, body);
        }

        public void Publish(string exchangeName, string routingKey, bool mandatory, byte[][] bodies)
        {
            var properties = Channel.GetHabbitMqProperties();

            var batch = Channel.CreateBasicPublishBatch();

            foreach (var body in bodies)
                batch.Add(exchangeName, routingKey, mandatory, properties, body);

            batch.Publish();
        }
    }
}
