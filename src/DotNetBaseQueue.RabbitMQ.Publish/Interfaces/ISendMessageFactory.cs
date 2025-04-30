using DotNetBaseQueue.Interfaces.Configs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotNetBaseQueue.RabbitMQ.Publicar.Interfaces
{
    public interface ISendMessageFactory
    {
        Task PublishAsync<T>(QueueHostConfiguration configuration, T entity = default, string exchangeName = "", string routingKey = "", bool mandatory = false, bool persistent = true);
        Task PublishListAsync<T>(QueueHostConfiguration configuration, IEnumerable<T> entities = default, string exchangeName = "", string routingKey = "", bool mandatory = false, bool persistent = true);
    }
}
