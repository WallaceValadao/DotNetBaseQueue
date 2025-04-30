using System.Threading.Tasks;
using DotNetBaseQueue.Interfaces.Configs;

namespace DotNetBaseQueue.RabbitMQ.Publicar.Interfaces
{
    public interface IRabbitMqConnectionFactory
    {
        Task<IConnectionPublish> GetConnectionAsync(QueueHostConfiguration rabbitHostConfiguration, string exchangeName, string routingKey, bool reconect = false);
    }
}
