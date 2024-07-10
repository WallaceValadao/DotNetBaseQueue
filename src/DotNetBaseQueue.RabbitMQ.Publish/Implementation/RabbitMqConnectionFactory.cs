using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using DotNetBaseQueue.Interfaces.Configs;
using DotNetBaseQueue.RabbitMQ.Publicar.Interfaces;

namespace DotNetBaseQueue.RabbitMQ.Publicar.Implementation
{
    public class RabbitMqConnectionFactory : IRabbitMqConnectionFactory
    {
        private readonly ILogger<RabbitMqConnectionFactory> _logger;
        private readonly ConcurrentDictionary<string, IConnectionPublish> connections;

        public RabbitMqConnectionFactory(ILogger<RabbitMqConnectionFactory> logger)
        {
            _logger = logger;
            this.connections = new ConcurrentDictionary<string, IConnectionPublish>();
        }

        public IConnectionPublish GetConnection(RabbitHostConfiguration rabbitHostConfiguration, string exchangeName, string routingKey, bool reconect = false)
        {
            var name = $"{rabbitHostConfiguration.HostName}-{rabbitHostConfiguration.Port}-{rabbitHostConfiguration.UserName}-{exchangeName}-{routingKey}";
            if (!connections.TryGetValue(name, out var connection))
            {
                connection = CreateConnection(rabbitHostConfiguration, name);
            }

            if (!reconect)
                return connection;

            try
            {
                connection.Dispose();
            }
            finally
            {
                connections.TryRemove(name, out var _);
            }

            return GetConnection(rabbitHostConfiguration, exchangeName, routingKey);
        }

        private IConnectionPublish CreateConnection(RabbitHostConfiguration rabbitHostConfiguration, string name)
        {
            IConnectionPublish connection;

            try
            {
                connection = new ConnectionPublish(rabbitHostConfiguration);
                connections.TryAdd(name, connection);
                _logger.LogInformation($"Created a new connection: {name}");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error opening connection with rabbit", ex);

                throw;
            }

            return connection;
        }
    }
}
