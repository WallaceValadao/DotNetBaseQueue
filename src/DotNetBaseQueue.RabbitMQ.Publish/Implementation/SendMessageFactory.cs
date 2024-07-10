using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using DotNetBaseQueue.Interfaces.Configs;
using DotNetBaseQueue.RabbitMQ.Core;
using DotNetBaseQueue.RabbitMQ.Publicar.Interfaces;

namespace DotNetBaseQueue.RabbitMQ.Publicar.Implementation
{
    public class SendMessageFactory : ISendMessageFactory
    {
        private readonly IRabbitMqConnectionFactory _rabbitMqConexaoFactory;
        private readonly ILogger<SendMessageFactory> _logger;

        public SendMessageFactory(IRabbitMqConnectionFactory rabbitMqConexaoFactory, ILogger<SendMessageFactory> logger)
        {
            _rabbitMqConexaoFactory = rabbitMqConexaoFactory;
            _logger = logger;
        }

        public void Publish<T>(QueueHostConfiguration configuration, T entity = default, string exchangeName = "", string routingKey = "", bool mandatory = false)
        {
            Publish(configuration, entity, exchangeName, routingKey, mandatory, false);
        }

        private void Publish<T>(QueueHostConfiguration configuration, T entity, string exchangeName, string routingKey, bool mandatory, bool reconnect)
        {
            try
            {
                var conexao = _rabbitMqConexaoFactory.GetConnection(configuration, exchangeName, routingKey, reconnect);

                var body = entity.GetMessage();

                conexao.Publish(exchangeName, routingKey, mandatory, body);
            }
            catch (Exception ex)
            {
                if (reconnect)
                {
                    _logger.LogError(ex, "Error sending message.");
                    throw;
                }

                Publish(configuration, entity, exchangeName, routingKey, mandatory, true);
            }
        }

        public void PublishList<T>(QueueHostConfiguration configuration, IEnumerable<T> entities = default, string exchangeName = "", string routingKey = "", bool mandatory = false)
        {
            PublishList(configuration, entities, exchangeName, routingKey, mandatory, false);
        }

        private void PublishList<T>(QueueHostConfiguration configuration, IEnumerable<T> entities, string exchangeName, string routingKey, bool mandatory, bool reconnect)
        {
            try
            {
                var conexao = _rabbitMqConexaoFactory.GetConnection(configuration, exchangeName, routingKey, reconnect);

                var bodies = entities.Select(x => x.GetMessage()).ToArray();

                conexao.Publish(exchangeName, routingKey, mandatory, bodies);
            }
            catch (Exception ex)
            {
                if (reconnect)
                {
                    _logger.LogError(ex, "Erro ao enviar mensagem");
                    throw;
                }

                PublishList(configuration, entities, exchangeName, routingKey, mandatory, true);
            }
        }
    }
}
