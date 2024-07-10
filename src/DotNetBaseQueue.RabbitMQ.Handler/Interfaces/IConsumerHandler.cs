using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetBaseQueue.RabbitMQ.Interfaces
{
    public interface IConsumerHandler
    {
        IEnumerable<Task> CreateTask(CancellationToken stoppingToken);
    }
}
