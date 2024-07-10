namespace DotNetBaseQueue.Interfaces.Event
{
    public interface IRabbitEventRetry : IRabbitEvent
    {
        int Retry { get; set; }
    }
}
