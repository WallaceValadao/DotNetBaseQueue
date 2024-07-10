namespace DotNetBaseQueue.Interfaces.Configs
{
    public class QueueInfoQueueConfiguration : QueueInfoQueuePublishConfiguration
    {
        public string ExchangeType { get; set; }
        public string QueueName { get; set; }
        
        public bool CreateDeadLetterQueue { get; set; } = false;
        public bool CreateRetryQueue { get; set; } = false;

        public int NumberOfWorkroles { get; set; } = 1;
        public ushort PrefetchCount { get; set; } = 1;
    }
}
