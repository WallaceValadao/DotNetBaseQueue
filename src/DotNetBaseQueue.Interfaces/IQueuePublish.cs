namespace DotNetBaseQueue.Interfaces
{
    using DotNetBaseQueue.Interfaces.Configs;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IQueuePublish
    {
        Task PublishAsync<T>(T entidade, string configPublishSectionQueueMQ);
        Task PublishAsync<T>(T entidade, QueueInfoQueuePublishConfiguration info);
        Task PublishAsync<T>(T item, string exchangeName, string routingKey);
        Task PublishAsync<T>(T entidade, QueueHostConfiguration QueueConfiguracao, QueueInfoQueuePublishConfiguration queueConfiguracao);
        Task PublishListAsync<T>(IEnumerable<T> entidade, string configPublishSectionQueue);
        Task PublishListAsync<T>(IEnumerable<T> entidade, QueueInfoQueuePublishConfiguration info);
        Task PublishListAsync<T>(IEnumerable<T> item, string exchangeName, string routingKey);
        Task PublishListAsync<T>(IEnumerable<T> entidade, QueueHostConfiguration queueHostConfiguracao, QueueInfoQueuePublishConfiguration queueConfiguracao);
    }
}
