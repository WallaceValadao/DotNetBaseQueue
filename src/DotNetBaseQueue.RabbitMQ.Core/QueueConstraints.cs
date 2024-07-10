namespace DotNetBaseQueue.RabbitMQ.Core
{
    public static class QueueConstraints
    {
        public static string DEAD_LETTER_EXCHANGE = "x-dead-letter-exchange";
        public static string DEAD_LETTER_ROUTING = "x-dead-letter-routing-key";
        public static string MESSAGE_DELAY = "x-message-ttl";

        public static string PATH_DEAD = "-dead";
        public static string PATH_RETRY = "-retry";
        public static string TYPE = "direct";
    }
}
