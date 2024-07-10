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

                if (entity is IRabbitEventRetry retry)
                    retry.Retry = Convert.ToInt32(retrys);
            }

            return entity;
        }
        
        public static void ValidateConfig(this RabbitConfiguration rabbitMQConfiguration)
        {
            if (rabbitMQConfiguration.Port < 1)
                CreateException(nameof(rabbitMQConfiguration.Port), "is less than 1");

            if (rabbitMQConfiguration.NumberOfWorkroles < 1) 
                CreateException(nameof(rabbitMQConfiguration.NumberOfWorkroles), "is less than 1");

            if (rabbitMQConfiguration.PrefetchCount < 1) 
                CreateException(nameof(rabbitMQConfiguration.PrefetchCount), "is less than 1");

            if (string.IsNullOrEmpty(rabbitMQConfiguration.HostName)) 
                CreateException(nameof(rabbitMQConfiguration.HostName), "is null or empty");

            if (string.IsNullOrEmpty(rabbitMQConfiguration.UserName)) 
                CreateException(nameof(rabbitMQConfiguration.UserName), "is null or empty");

            if (string.IsNullOrEmpty(rabbitMQConfiguration.Password)) 
                CreateException(nameof(rabbitMQConfiguration.Password), "is null or empty");

            if (string.IsNullOrEmpty(rabbitMQConfiguration.ExchangeName)) 
                CreateException(nameof(rabbitMQConfiguration.ExchangeName), "is null or empty");

            if (string.IsNullOrEmpty(rabbitMQConfiguration.ExchangeType)) 
                CreateException(nameof(rabbitMQConfiguration.ExchangeType), "is null or empty");

            if (string.IsNullOrEmpty(rabbitMQConfiguration.RoutingKey)) 
                CreateException(nameof(rabbitMQConfiguration.RoutingKey), "is null or empty");
                
            if (string.IsNullOrEmpty(rabbitMQConfiguration.QueueName)) 
                CreateException(nameof(rabbitMQConfiguration.QueueName), "is null or empty");
        }

        internal static void CreateException(string nomeParametro, string regra)
        {
            throw new ArgumentException($"RabbitMQ Subscribe configuration is incorrect, {nomeParametro} {regra}.", nomeParametro);
        }
    }
}
