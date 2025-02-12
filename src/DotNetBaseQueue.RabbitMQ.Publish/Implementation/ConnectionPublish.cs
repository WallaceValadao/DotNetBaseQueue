using RabbitMQ.Client;
using DotNetBaseQueue.Interfaces.Configs;
using DotNetBaseQueue.RabbitMQ.Core;
using DotNetBaseQueue.RabbitMQ.Publicar.Interfaces;
using System.Collections.Generic;

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

        public void Publish(string exchangeName, string routingKey, bool mandatory, byte[] body, string correlationId)
        {
            var properties = GetHabbitMqProperties(correlationId);

            Channel.BasicPublish(exchangeName, routingKey, mandatory, properties, body);
        }

        private IBasicProperties GetHabbitMqProperties(string correlationId)
        {
            var properties = Channel.GetHabbitMqProperties();

            properties.Headers ??= new Dictionary<string, object>();

            properties.Headers.Add(QueueConstraints.CORRELATION_ID_HEADER, correlationId);

            return properties;
        }

        public void Publish(string exchangeName, string routingKey, bool mandatory, byte[][] bodies, string correlationId)
        {
            var properties = GetHabbitMqProperties(correlationId);

            var batch = Channel.CreateBasicPublishBatch();

            foreach (var body in bodies)
                batch.Add(exchangeName, routingKey, mandatory, properties, body);

            batch.Publish();
        }
    }
}
