using DotNetBaseQueue.Interfaces.Event;

namespace DotNetBaseQueue.Api.Sample;

public class ExampleMessage : IQueueEvent
{
    public string Id { get; set; }
}
