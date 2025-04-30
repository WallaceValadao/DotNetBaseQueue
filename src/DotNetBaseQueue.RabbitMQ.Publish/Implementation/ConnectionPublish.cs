using RabbitMQ.Client;
using DotNetBaseQueue.Interfaces.Configs;
using DotNetBaseQueue.RabbitMQ.Core;
using DotNetBaseQueue.RabbitMQ.Publicar.Interfaces;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace DotNetBaseQueue.RabbitMQ.Publicar.Implementation
{
    public class ConnectionPublish : IConnectionPublish, IAsyncDisposable
    {
        public readonly ConnectionFactory connectionFactory;

        public IConnection connection;
        public IChannel channel { get; private set; }

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
        }

        private async Task<IChannel> ChannelAsync()
        {
            if (channel != null)
                return channel;

            connection = await connectionFactory.CreateConnectionAsync();
            channel = await connection.CreateChannelAsync();

            return channel;
        }

        public async Task PublishAsync(string exchangeName, string routingKey, bool mandatory, byte[] body, string correlationId, bool persistent)
        {
            var properties = MessageHelper.GetHabbitMqProperties(correlationId, persistent);

            var Channel = await ChannelAsync();
            await Channel.BasicPublishAsync(exchangeName, routingKey, mandatory, properties, body);
        }

        

        public async Task PublishAsync(string exchangeName, string routingKey, bool mandatory, byte[][] bodies, string correlationId, bool persistent)
        {
            var properties = MessageHelper.GetHabbitMqProperties(correlationId, persistent);

            var Channel = await ChannelAsync();

            foreach (var body in bodies)
                await Channel.BasicPublishAsync(exchangeName, routingKey, mandatory, properties, body);
        }

        public async ValueTask DisposeAsync()
        {
            await channel.CloseAsync();
            await connection.CloseAsync();
        }
    }
}
