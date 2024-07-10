namespace DotNetBaseQueue.Interfaces.Configs
{
    public class QueueInfoQueuePublishConfiguration
    {
        public string ExchangeName { get; set; }
        public string RoutingKey { get; set; }
        public int SecondsToRetry { get; set; } = 2;
    }
}
