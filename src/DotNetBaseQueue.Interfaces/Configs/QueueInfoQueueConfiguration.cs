namespace DotNetBaseQueue.Interfaces.Configs
{
    public class QueueInfoQueueConfiguration : QueueInfoQueuePublishConfiguration
    {
        public string ExchangeType { get; set; } = "direct";
        public string QueueName { get; set; }
        public string[] RoutingKeys { get; set; }
        
        public bool CreateDeadLetterQueue { get; set; } = true;
        public bool CreateRetryQueue { get; set; } = true;
        public int NumberTryRetry { get; set; } = 3;
        public int SecondsToRetry { get; set; } = 2;

        public int NumberOfWorkroles { get; set; } = 1;
        public ushort PrefetchCount { get; set; } = 1;
    }
}
