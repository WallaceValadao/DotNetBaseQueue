using DotNetBaseQueue.Interfaces.Configs;
using DotNetBaseQueue.Interfaces.Event;

namespace DotNetBaseQueue.RabbitMQ.HostService
{
    public class ConsumerConfiguration<IEntidade, IEvent>(QueueConfiguration queueMQConfiguration) where IEntidade : class, IQueueEvent where IEvent : class, IQueueEventHandler<IEntidade>
    {
        public QueueConfiguration QueueConfiguration { get; } = queueMQConfiguration;
    }
}
