﻿using RabbitMQ.Client;
using DotNetBaseQueue.RabbitMQ.Core;
using DotNetBaseQueue.RabbitMQ.Consumir.Exceptions;
using System;
using System.Collections.Generic;

namespace DotNetBaseQueue.RabbitMQ.Handler.Extensions
{
    internal static class RabbitMqModelExtension
    {
        public static Dictionary<string, object> CreateDeadLetterQueue(this IModel channel, string letterExchange, string letterQueue, bool deleteQueue)
        {
            string deadLetterExchange = letterExchange;
            string deadLetterRoutingKey = $"{letterQueue}{QueueConstraints.PATH_DEAD}";
            string deadLetterQueue = $"{letterQueue}{QueueConstraints.PATH_DEAD}";

            if (deleteQueue)
                DeleteQueue(channel, deadLetterQueue);

            channel.ExchangeDeclare(deadLetterExchange, QueueConstraints.TYPE, true);

            QueueDeclare(channel, deadLetterQueue);
            channel.QueueBind(queue: deadLetterQueue,
                            exchange: deadLetterExchange,
                            routingKey: deadLetterRoutingKey);

            return new Dictionary<string, object>()
            {
                { QueueConstraints.DEAD_LETTER_EXCHANGE, deadLetterExchange },
                { QueueConstraints.DEAD_LETTER_ROUTING, deadLetterRoutingKey }
            };
        }

        private static void DeleteQueue(IModel channel, string deadLetterQueue)
        {
            var queuePassive = channel.QueueDeclarePassive(deadLetterQueue);

            if (queuePassive.MessageCount > 0)
                throw new QueueDeadInvalidConfigurationException($"Error when recreating queue with messages to process: {deadLetterQueue}");

            channel.QueueDelete(deadLetterQueue);
        }

        private static void QueueDeclare(IModel channel, string deadLetterQueue)
        {
            try
            {
                channel.QueueDeclare(queue: deadLetterQueue,
                                                 durable: true,
                                                 exclusive: false,
                                                 autoDelete: false);
            }
            catch (Exception ex)
            {
                throw new QueueDeadInvalidConfigurationException($"Error declaring queue: {deadLetterQueue}", ex);
            }
        }

        public static void CreateRetryQueue(this IModel channel, string letterExchange, string letterRoutingKey, string letterQueue, int secondsToRetry = 2)
        {
            var retryLetterExchange = letterExchange;
            var retryLetterRoutingKey = $"{letterQueue}{QueueConstraints.PATH_RETRY}";
            var retryLetterQueue = $"{letterQueue}{QueueConstraints.PATH_RETRY}";
            var retryRouteKey =  $"{letterQueue}{QueueConstraints.PATH_RETRY_PUB}";

            channel.ExchangeDeclare(retryLetterExchange, QueueConstraints.TYPE, true);
            channel.QueueDeclare(retryLetterQueue, true, false, false, GetParametersRetry(letterExchange, letterRoutingKey, secondsToRetry));
            channel.QueueBind(queue: retryLetterQueue,
                            exchange: retryLetterExchange,
                            routingKey: retryLetterRoutingKey);

            if (letterQueue != letterRoutingKey)
            {
                channel.QueueBind(queue: letterQueue, exchange: letterExchange, retryRouteKey);
            }
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
