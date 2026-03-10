using System.Diagnostics;

namespace DotNetBaseQueue.RabbitMQ.Handler.Telemetry
{
    internal static class RabbitMqActivitySource
    {
        internal const string SourceName = "DotNetBaseQueue.RabbitMQ";

        internal static readonly ActivitySource Source = new(SourceName, "1.0.0");
    }
}
