using DotNetBaseQueue.Interfaces.Configs;

namespace DotNetBaseQueue.RabbitMQ.Publish.Interfaces
{
    public interface IQueueAppConfiguration
    {
        (QueueHostConfiguration QueueConfiguration, QueueInfoQueuePublishConfiguration queueConfiguration) GetConfiguration(string configPublishSectionQueueMQ);
    }
}