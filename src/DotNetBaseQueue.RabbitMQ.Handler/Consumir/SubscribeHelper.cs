using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using DotNetBaseQueue.RabbitMQ.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using DotNetBaseQueue.Interfaces.Configs;
using DotNetBaseQueue.Interfaces.Event;

namespace DotNetBaseQueue.RabbitMQ.Handler.Consumir
{
    internal static class SubscribeHelper
    {
        internal static T GetEntityQueue<T>(this BasicDeliverEventArgs basicDeliverEventArgs, IModel channel)
        {
            var entity = basicDeliverEventArgs.Body.GetMessage<T>();

            if (basicDeliverEventArgs.BasicProperties != null && basicDeliverEventArgs.BasicProperties.Headers != null && basicDeliverEventArgs.BasicProperties.Headers.Any())
            {
                var listaHeader = basicDeliverEventArgs.BasicProperties.Headers["x-death"] as List<Object>;
                var dictionaryItem = listaHeader[0] as Dictionary<string, object>;

                var retrys = dictionaryItem["count"];
                if ((long)retrys > 100)
                {
                    channel.BasicAck(deliveryTag: basicDeliverEventArgs.DeliveryTag, multiple: false);
                    throw new Exception();
                }

                if (entity is IQueueEventRetry retry)
                    retry.Retry = Convert.ToInt32(retrys);
            }

            return entity;
        }
        
        public static void ValidateConfig(this QueueConfiguration queueConfiguration)
        {
            if (queueConfiguration.Port < 1)
                CreateException(nameof(queueConfiguration.Port), "is less than 1");

            if (queueConfiguration.NumberOfWorkroles < 1) 
                CreateException(nameof(queueConfiguration.NumberOfWorkroles), "is less than 1");

            if (queueConfiguration.PrefetchCount < 1) 
                CreateException(nameof(queueConfiguration.PrefetchCount), "is less than 1");

            if (string.IsNullOrEmpty(queueConfiguration.HostName)) 
                CreateException(nameof(queueConfiguration.HostName), "is null or empty");

            if (string.IsNullOrEmpty(queueConfiguration.UserName)) 
                CreateException(nameof(queueConfiguration.UserName), "is null or empty");

            if (string.IsNullOrEmpty(queueConfiguration.Password)) 
                CreateException(nameof(queueConfiguration.Password), "is null or empty");

            if (string.IsNullOrEmpty(queueConfiguration.ExchangeName)) 
                CreateException(nameof(queueConfiguration.ExchangeName), "is null or empty");

            if (string.IsNullOrEmpty(queueConfiguration.ExchangeType)) 
                CreateException(nameof(queueConfiguration.ExchangeType), "is null or empty");

            if (string.IsNullOrEmpty(queueConfiguration.RoutingKey)) 
                CreateException(nameof(queueConfiguration.RoutingKey), "is null or empty");
                
            if (string.IsNullOrEmpty(queueConfiguration.QueueName)) 
                CreateException(nameof(queueConfiguration.QueueName), "is null or empty");
        }

        internal static void CreateException(string nameParameter, string rule)
        {
            throw new ArgumentException($"RabbitMQ Subscribe configuration is incorrect, {nameParameter} {rule}.", nameParameter);
        }
    }
}
