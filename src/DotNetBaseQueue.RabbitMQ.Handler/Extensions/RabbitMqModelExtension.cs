using RabbitMQ.Client;
using DotNetBaseQueue.RabbitMQ.Core;
using DotNetBaseQueue.RabbitMQ.Consumir.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotNetBaseQueue.RabbitMQ.Handler.Extensions
{
    internal static class RabbitMqModelExtension
    {
        public static async Task<Dictionary<string, object>> CreateDeadLetterQueueAsync(this IChannel channel, string letterExchange, string letterQueue, bool deleteQueue)
        {
            string deadLetterExchange = letterExchange;
            string deadLetterRoutingKey = $"{letterQueue}{QueueConstraints.PATH_DEAD}";
            string deadLetterQueue = $"{letterQueue}{QueueConstraints.PATH_DEAD}";

            if (deleteQueue)
                await DeleteQueueAsync(channel, deadLetterQueue);

            await channel.ExchangeDeclareAsync(deadLetterExchange, QueueConstraints.TYPE, true);

            await QueueDeclareAsync(channel, deadLetterQueue);
            await channel.QueueBindAsync(queue: deadLetterQueue,
                            exchange: deadLetterExchange,
                            routingKey: deadLetterRoutingKey);

            return new Dictionary<string, object>()
            {
                { QueueConstraints.DEAD_LETTER_EXCHANGE, deadLetterExchange },
                { QueueConstraints.DEAD_LETTER_ROUTING, deadLetterRoutingKey }
            };
        }

        private static async Task DeleteQueueAsync(IChannel channel, string deadLetterQueue)
        {
            var queuePassive = await channel.QueueDeclarePassiveAsync(deadLetterQueue);

            if (queuePassive.MessageCount > 0)
                throw new QueueDeadInvalidConfigurationException($"Error when recreating queue with messages to process: {deadLetterQueue}");

            await channel.QueueDeleteAsync(deadLetterQueue);
        }

        private static async Task QueueDeclareAsync(IChannel channel, string deadLetterQueue)
        {
            try
            {
                await channel.QueueDeclareAsync(queue: deadLetterQueue,
                                                 durable: true,
                                                 exclusive: false,
                                                 autoDelete: false);
            }
            catch (Exception ex)
            {
                throw new QueueDeadInvalidConfigurationException($"Error declaring queue: {deadLetterQueue}", ex);
            }
        }

        public static async Task CreateRetryQueueAsync(this IChannel channel, string letterExchange, string letterRoutingKey, string letterQueue, int secondsToRetry = 2)
        {
            var retryLetterExchange = letterExchange;
            var retryLetterRoutingKey = $"{letterQueue}{QueueConstraints.PATH_RETRY}";
            var retryLetterQueue = $"{letterQueue}{QueueConstraints.PATH_RETRY}";
            var retryRouteKey = $"{letterQueue}{QueueConstraints.PATH_RETRY_PUB}";

            await channel.ExchangeDeclareAsync(retryLetterExchange, QueueConstraints.TYPE, true);
            await channel.QueueDeclareAsync(retryLetterQueue, true, false, false, GetParametersRetry(letterExchange, retryRouteKey, secondsToRetry));
            await channel.QueueBindAsync(queue: retryLetterQueue,
                            exchange: retryLetterExchange,
                            routingKey: retryLetterRoutingKey);
            await channel.QueueBindAsync(queue: letterQueue, exchange: letterExchange, retryRouteKey);
        }

        private static Dictionary<string, object> GetParametersRetry(string exchange, string retryRouteKey, int delay)
        {
            return new Dictionary<string, object>
            {
                { QueueConstraints.DEAD_LETTER_EXCHANGE, exchange},
                { QueueConstraints.DEAD_LETTER_ROUTING, retryRouteKey},
                { QueueConstraints.MESSAGE_DELAY, delay * 1000 }
            };
        }
    }
}
