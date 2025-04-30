using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using DotNetBaseQueue.Interfaces.Configs;
using DotNetBaseQueue.RabbitMQ.Core;
using DotNetBaseQueue.RabbitMQ.Publicar.Interfaces;
using DotNetBaseQueue.Interfaces.Logs;
using System.Threading.Tasks;

namespace DotNetBaseQueue.RabbitMQ.Publicar.Implementation
{
    public class SendMessageFactory : ISendMessageFactory
    {
        private readonly IRabbitMqConnectionFactory _rabbitMqConexaoFactory;
        private readonly ILogger<SendMessageFactory> _logger;
        private readonly ICorrelationIdService _correlationIdService;

        public SendMessageFactory(IRabbitMqConnectionFactory rabbitMqConexaoFactory, ILogger<SendMessageFactory> logger, ICorrelationIdService correlationIdService)
        {
            _rabbitMqConexaoFactory = rabbitMqConexaoFactory;
            _logger = logger;
            _correlationIdService = correlationIdService;
        }

        public Task PublishAsync<T>(QueueHostConfiguration configuration, T entity = default, string exchangeName = "", string routingKey = "", bool mandatory = false, bool persistent = true)
        {
            return PublishAsync(configuration, entity, exchangeName, routingKey, mandatory, persistent, false);
        }

        private async Task PublishAsync<T>(QueueHostConfiguration configuration, T entity, string exchangeName, string routingKey, bool mandatory, bool persistent, bool reconnect)
        {
            try
            {
                var conexao = await _rabbitMqConexaoFactory.GetConnectionAsync(configuration, exchangeName, routingKey, reconnect);

                var body = entity.GetMessage();

                await conexao.PublishAsync(exchangeName, routingKey, mandatory, body, _correlationIdService.Get(), persistent);
            }
            catch (Exception ex)
            {
                if (reconnect)
                {
                    _logger.LogError(ex, "Error sending message.");
                    throw;
                }

                await PublishAsync(configuration, entity, exchangeName, routingKey, mandatory, persistent, true);
            }
        }

        public Task PublishListAsync<T>(QueueHostConfiguration configuration, IEnumerable<T> entities = default, string exchangeName = "", string routingKey = "", bool mandatory = false, bool persistent = true)
        {
            return PublishListAsync(configuration, entities, exchangeName, routingKey, mandatory, false);
        }

        private async Task PublishListAsync<T>(QueueHostConfiguration configuration, IEnumerable<T> entities, string exchangeName, string routingKey, bool mandatory, bool persistent, bool reconnect)
        {
            try
            {
                var conexao = await _rabbitMqConexaoFactory.GetConnectionAsync(configuration, exchangeName, routingKey, reconnect);

                var bodies = entities.Select(x => x.GetMessage()).ToArray();

                await conexao.PublishAsync(exchangeName, routingKey, mandatory, bodies, _correlationIdService.Get(), persistent);
            }
            catch (Exception ex)
            {
                if (reconnect)
                {
                    _logger.LogError(ex, "Erro ao enviar mensagem");
                    throw;
                }

                await PublishListAsync(configuration, entities, exchangeName, routingKey, mandatory, persistent, true);
            }
        }
    }
}
