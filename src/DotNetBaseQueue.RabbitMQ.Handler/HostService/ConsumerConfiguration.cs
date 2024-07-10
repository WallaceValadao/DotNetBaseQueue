using DotNetBaseQueue.Interfaces.Configs;
using DotNetBaseQueue.Interfaces.Event;

namespace DotNetBaseQueue.RabbitMQ.HostService
{
    public class ConsumerConfiguration<IEntidade, IEvent> where IEntidade : class, IRabbitEvent where IEvent : class, IRabbitEventHandler<IEntidade>
    {
        public RabbitConfiguration RabbitMQConfiguration { get; }

        public ConsumerConfiguration(RabbitConfiguration rabbitMQConfiguration)
        {
            this.RabbitMQConfiguration = rabbitMQConfiguration;
        }
    }
}
