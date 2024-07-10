namespace DotNetBaseQueue.Interfaces.Event
{
    using System.Threading.Tasks;

    public interface IRabbitEventHandler<T> where T : class, IRabbitEvent
    {
        Task HandleAsync(T command);
    }
}
