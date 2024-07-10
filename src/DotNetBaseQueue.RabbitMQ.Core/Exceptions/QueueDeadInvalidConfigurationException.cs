using System;

namespace DotNetBaseQueue.RabbitMQ.Consumir.Exceptions
{
    public class QueueDeadInvalidConfigurationException : Exception
    {
        public QueueDeadInvalidConfigurationException()
        {
        }

        public QueueDeadInvalidConfigurationException(string message)
        : base(message)
        {
        }

        public QueueDeadInvalidConfigurationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
