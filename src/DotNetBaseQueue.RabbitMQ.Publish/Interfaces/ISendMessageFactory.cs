using DotNetBaseQueue.Interfaces.Configs;
using System.Collections.Generic;

namespace DotNetBaseQueue.RabbitMQ.Publicar.Interfaces
{
    public interface ISendMessageFactory
    {
        void Publish<T>(RabbitHostConfiguration configuration, T entity = default, string exchangeName = "", string routingKey = "", bool mandatory = false);
        void PublishList<T>(RabbitHostConfiguration configuration, IEnumerable<T> entities = default, string exchangeName = "", string routingKey = "", bool mandatory = false);
    }
}
