namespace DotNetBaseQueue.RabbitMQ.Core
{
    public static class QueueConstraints
    {
        public const string DEAD_LETTER_EXCHANGE = "x-dead-letter-exchange";
        public const string DEAD_LETTER_ROUTING = "x-dead-letter-routing-key";
        public const string MESSAGE_DELAY = "x-message-ttl";

        public const string PATH_RETRY_PUB = "-retry-pub";

        public const string PATH_DEAD = "-dead";
        public const string PATH_RETRY = "-retry";
        public const string TYPE = "direct";
    }
}
