using System;

namespace DotNetBaseQueue.RabbitMQ.Handler
{
    public class CorrelationIdService : ICorrelationIdService
    {
        private readonly string correlationId;

        public CorrelationIdService()
        {
            correlationId = Guid.NewGuid().ToString().Replace("-", string.Empty);
        }

        public string Get()
        {
            return correlationId;
        }
    }
}
