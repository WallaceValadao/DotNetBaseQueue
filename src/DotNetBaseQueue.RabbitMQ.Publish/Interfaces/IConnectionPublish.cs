using System;
using System.Threading.Tasks;

namespace DotNetBaseQueue.RabbitMQ.Publicar.Interfaces
{
    public interface IConnectionPublish : IAsyncDisposable
    {
        Task PublishAsync(string exchangeName, string routingKey, bool mandatory, byte[] body, string correlationId, bool persistent);
        Task PublishAsync(string exchangeName, string routingKey, bool mandatory, byte[][] bodies, string correlationId, bool persistent);
    }
}
