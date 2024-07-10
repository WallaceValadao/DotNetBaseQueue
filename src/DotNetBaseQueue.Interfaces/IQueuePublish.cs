namespace DotNetBaseQueue.Interfaces
{
    using DotNetBaseQueue.Interfaces.Configs;
    using System.Collections.Generic;

    public interface IQueuePublish
    {
        void Publish<T>(T entidade, string configPublishSectionQueueMQ);
        void Publish<T>(T entidade, QueueInfoQueuePublishConfiguration info);
        void Publish<T>(T item, string exchangeName, string routingKey);
        void Publish<T>(T entidade, QueueHostConfiguration QueueConfiguracao, QueueInfoQueuePublishConfiguration queueConfiguracao);
        void PublishList<T>(IEnumerable<T> entidade, string configPublishSectionQueue);
        void PublishList<T>(IEnumerable<T> entidade, QueueInfoQueuePublishConfiguration info);
        void PublishList<T>(IEnumerable<T> item, string exchangeName, string routingKey);
        void PublishList<T>(IEnumerable<T> entidade, QueueHostConfiguration queueHostConfiguracao, QueueInfoQueuePublishConfiguration queueConfiguracao);
    }
}
