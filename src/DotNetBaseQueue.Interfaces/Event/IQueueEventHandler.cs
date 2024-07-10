namespace DotNetBaseQueue.Interfaces.Event
{
    using System.Threading.Tasks;

    public interface IQueueEventHandler<T> where T : class, IQueueEvent
    {
        Task HandleAsync(T command);
    }
}
