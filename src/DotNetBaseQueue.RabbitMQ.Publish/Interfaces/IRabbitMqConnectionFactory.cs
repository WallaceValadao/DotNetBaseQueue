using DotNetBaseQueue.Interfaces.Configs;

namespace DotNetBaseQueue.RabbitMQ.Publicar.Interfaces
{
    public interface IRabbitMqConnectionFactory
    {
        IConnectionPublish GetConnection(QueueHostConfiguration rabbitHostConfiguration, string exchangeName, string routingKey, bool reconect = false);
    }
}
