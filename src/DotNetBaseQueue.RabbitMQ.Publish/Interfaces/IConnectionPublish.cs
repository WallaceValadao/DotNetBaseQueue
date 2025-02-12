using System;

namespace DotNetBaseQueue.RabbitMQ.Publicar.Interfaces
{
    public interface IConnectionPublish : IDisposable
    {
        void Publish(string exchangeName, string routingKey, bool mandatory, byte[] body, string correlationId);
        void Publish(string exchangeName, string routingKey, bool mandatory, byte[][] bodies, string correlationId);
    }
}
