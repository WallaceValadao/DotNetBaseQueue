using RabbitMQ.Client;
using System;
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

        public static IBasicProperties GetHabbitMqProperties(this IModel channel)
        {
            var properties = channel.CreateBasicProperties();
            //Seta a mensagem como persistente
            properties.Persistent = true;

            return properties;
        }
    }
}
