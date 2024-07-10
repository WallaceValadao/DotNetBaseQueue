using DotNetBaseQueue.Interfaces.Configs;

namespace DotNetBaseQueue.RabbitMQ.Publicar.Interfaces
{
    public interface IRabbitMqConnectionFactory
    {
        IConnectionPublish GetConnection(RabbitHostConfiguration rabbitHostConfiguration, string exchangeName, string routingKey, bool reconect = false);
    }
}
