using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DotNetBaseQueue.RabbitMQ.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;

namespace DotNetBaseQueue.RabbitMQ.HostService
{
    public class ConsumerHostedService : BackgroundService
    {

        private readonly IEnumerable<IConsumerHandler> commandHandlers;
        private readonly IHostApplicationLifetime hostApplicationLifetime;
        private readonly TelemetryClient telemetryClient;
        private readonly ILogger<ConsumerHostedService> logger;

        public ConsumerHostedService(IEnumerable<IConsumerHandler> commandHandlers,
                                IHostApplicationLifetime hostApplicationLifetime,
                                TelemetryClient telemetryClient,
                                ILogger<ConsumerHostedService> logger)
        {
            this.commandHandlers = commandHandlers;
            this.hostApplicationLifetime = hostApplicationLifetime;
            this.telemetryClient = telemetryClient;
            this.logger = logger;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Started consumer service with {HandlerCount} handler(s)", commandHandlers.Count());

            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var tasks = commandHandlers.SelectMany(x => x.CreateTask(stoppingToken));

            return Task.WhenAll(tasks);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Stopping consumer service");
            hostApplicationLifetime.StopApplication();
            await telemetryClient.FlushAsync(cancellationToken);
            await base.StopAsync(cancellationToken);
        }
    }
}
