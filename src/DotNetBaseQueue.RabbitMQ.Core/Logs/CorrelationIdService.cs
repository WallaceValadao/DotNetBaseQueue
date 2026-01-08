using System;
using DotNetBaseQueue.Interfaces.Logs;

namespace DotNetBaseQueue.RabbitMQ.Core.Logs
{
    public class CorrelationIdService : ICorrelationIdService
    {
        private string correlationId;

        public CorrelationIdService()
        {
            correlationId = Guid.NewGuid().ToString("N");
        }

        public string Get()
        {
            return correlationId;
        }

        public void Set(string correlationId)
        {
            this.correlationId = correlationId.Replace("-", string.Empty);
        }
    }
}