using DotNetBaseQueue.Interfaces.Configs;
using DotNetBaseQueue.Interfaces.Event;

namespace DotNetBaseQueue.RabbitMQ.HostService
{
    public class ConsumerConfiguration<IEntidade, IEvent> where IEntidade : class, IQueueEvent where IEvent : class, IQueueEventHandler<IEntidade>
    {
        public QueueConfiguration QueueConfiguration { get; }

        public ConsumerConfiguration(QueueConfiguration queueMQConfiguration)
        {
            this.QueueConfiguration = queueMQConfiguration;
        }
    }
}
