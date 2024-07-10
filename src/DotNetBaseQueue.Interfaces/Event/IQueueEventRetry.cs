namespace DotNetBaseQueue.Interfaces.Event
{
    public interface IQueueEventRetry : IQueueEvent
    {
        int Retry { get; set; }
    }
}
