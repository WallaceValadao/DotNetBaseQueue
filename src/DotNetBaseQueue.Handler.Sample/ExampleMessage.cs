using DotNetBaseQueue.Interfaces.Event;

namespace DotNetBaseQueue.Handler.Sample;

public class ExampleMessage : IQueueEvent
{
    public string Id { get; set; }
}
