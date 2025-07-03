using DotNetBaseQueue.Interfaces.Configs;
using DotNetBaseQueue.Interfaces.Event;

namespace DotNetBaseQueue.RabbitMQ.HostService
{
    public class ConsumerConfiguration<IEntidade, IEvent> where IEntidade : class, IQueueEvent where IEvent : class, IQueueEventHandler<IEntidade>
    {
        public ConsumerConfiguration(QueueConfiguration queueMQConfiguration)
        {
            QueueConfiguration = queueMQConfiguration;
        }

        public QueueConfiguration QueueConfiguration { get; }
    }
}
