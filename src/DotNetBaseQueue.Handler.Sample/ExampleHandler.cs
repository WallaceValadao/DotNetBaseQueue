using DotNetBaseQueue.Interfaces.Event;

namespace DotNetBaseQueue.Handler.Sample;

public class ExampleHandler : IQueueEventHandler<ExampleMessage>
{
    public Task HandleAsync(ExampleMessage command)
    {
        Console.WriteLine(command.Id);
        return Task.CompletedTask;
    }
}