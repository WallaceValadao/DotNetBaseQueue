using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetBaseQueue.RabbitMQ.Handler
{
    public class CorrelationIdLogger<T> : ILogger<T>
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider serviceProvider;

        public CorrelationIdLogger(IServiceProvider serviceProvider, ILoggerProvider loggerProvider)
        {
            this.serviceProvider = serviceProvider;

            _logger = loggerProvider.CreateLogger(nameof(T));
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            return _logger.BeginScope(new Dictionary<string, object>() { ["CorrelationId"] = state });
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _logger.IsEnabled(logLevel);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            _logger.Log(logLevel, exception, formatter(state, exception));
        }
    }
}
