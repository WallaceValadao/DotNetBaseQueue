using DotNetBaseQueue.Interfaces.Event;

namespace DotNetBaseQueue.Handler.Sample;

public class ExampleMessage : IQueueEventRetry
{
    public string Id { get; set; }
    public int Retry { get; set; }
}
