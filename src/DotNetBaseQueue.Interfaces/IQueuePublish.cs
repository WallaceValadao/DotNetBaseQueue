namespace DotNetBaseQueue.Interfaces
{
    using DotNetBaseQueue.Interfaces.Configs;
    using System.Collections.Generic;

    public interface IQueuePublish
    {
        void Publish<T>(T entidade, string configPublishSectionRabbitMQ);
        void Publish<T>(T entidade, RabbitInfoQueuePublishConfiguration info);
        void Publish<T>(T item, string exchangeName, string routingKey);
        void Publish<T>(T entidade, RabbitHostConfiguration rabbitConfiguracao, RabbitInfoQueuePublishConfiguration queueConfiguracao);
        void PublishList<T>(IEnumerable<T> entidade, string configPublishSectionRabbitMQ);
        void PublishList<T>(IEnumerable<T> entidade, RabbitInfoQueuePublishConfiguration info);
        void PublishList<T>(IEnumerable<T> item, string exchangeName, string routingKey);
        void PublishList<T>(IEnumerable<T> entidade, RabbitHostConfiguration rabbitConfiguracao, RabbitInfoQueuePublishConfiguration queueConfiguracao);
    }
}
