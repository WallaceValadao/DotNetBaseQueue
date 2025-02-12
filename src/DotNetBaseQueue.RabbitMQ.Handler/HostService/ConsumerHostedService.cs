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
        private readonly IEnumerable<IConsumerHandler> _consumerHandlers;
        private readonly ILogger<ConsumerHostedService> _logger;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly TelemetryClient _telemetryClient;

        public ConsumerHostedService(IEnumerable<IConsumerHandler> commandHandler,
                                    IHostApplicationLifetime hostApplicationLifetime,   
                                    TelemetryClient telemetryClient,
                                    ILogger<ConsumerHostedService> logger)
        {
            _consumerHandlers = commandHandler;
            _hostApplicationLifetime = hostApplicationLifetime;
            _logger = logger;
            _telemetryClient = telemetryClient;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Started consumer in queue.");

            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var tasks = _consumerHandlers.SelectMany(x => x.CreateTask(stoppingToken));

            return Task.WhenAll(tasks);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Finishing consumer from the queue.");
            _hostApplicationLifetime.StopApplication();
            await _telemetryClient.FlushAsync(cancellationToken);
            await base.StopAsync(cancellationToken);
        }
    }
}
