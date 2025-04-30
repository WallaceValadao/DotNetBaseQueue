using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace DotNetBaseQueue.RabbitMQ.Core
{
    public static class MessageHelper
    {
        public static T GetMessage<T>(this ReadOnlyMemory<byte> readOnlyMemory)
        {
            var body = Encoding.UTF8.GetString(readOnlyMemory.ToArray());

            var entidade = JsonSerializer.Deserialize<T>(body);

            return entidade;
        }

        public static byte[] GetMessage(this object entidade)
        {
            if (entidade == null)
                throw new ArgumentNullException(nameof(entidade));

            var message = JsonSerializer.Serialize(entidade);
            var body = Encoding.UTF8.GetBytes(message);

            return body;
        }
        
        public static BasicProperties GetHabbitMqProperties(string correlationId, bool persistent)
        {
            var properties = new BasicProperties
            {
                ContentType = "text/plain",
                DeliveryMode = persistent ? DeliveryModes.Persistent : DeliveryModes.Transient
            };
            properties.Headers ??= new Dictionary<string, object>();

            properties.Headers.Add(QueueConstraints.CORRELATION_ID_HEADER, correlationId);

            return properties;
        }
    }
}
