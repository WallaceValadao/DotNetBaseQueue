using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using DotNetBaseQueue.RabbitMQ.Consumir.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using DotNetBaseQueue.Interfaces.Configs;
using DotNetBaseQueue.RabbitMQ.Handler.Extensions;
using System.Threading.Tasks;

namespace DotNetBaseQueue.RabbitMQ.Handler.Consumir
{
    internal static class ConnectionHandler
    {
        internal static Task<IChannel> CreateConnectionAsync(QueueConfiguration queueConfiguration, ILogger logger)
        {
            return CreateModelAsync(queueConfiguration, logger);
        }

        internal static async Task<IChannel> CreateModelAsync(QueueConfiguration queueConfiguration, ILogger logger, bool reconnect = false, bool deleteQueueDead = false)
        {
            try
            {
                var connection = await CreateConnectionAsync(queueConfiguration, logger, reconnect);
                var channel = await connection.CreateChannelAsync();
                await channel.ExchangeDeclareAsync(exchange: queueConfiguration.ExchangeName, type: queueConfiguration.ExchangeType, durable: true);

                channel.ChannelShutdownAsync += (sender, ea) =>
                {
                    logger.LogError("Channel shutdown: {@ShutdownEventArgs}", ea);
                    return Task.CompletedTask;
                };

                Dictionary<string, object> args = null;
                if (queueConfiguration.CreateDeadLetterQueue)
                    args = await channel.CreateDeadLetterQueueAsync(queueConfiguration.ExchangeName, queueConfiguration.QueueName, deleteQueueDead);

                await channel.QueueDeclareAsync(queue: queueConfiguration.QueueName, durable: true, exclusive: false, autoDelete: false, arguments: args);
                await channel.QueueBindAsync(queue: queueConfiguration.QueueName, exchange: queueConfiguration.ExchangeName, routingKey: queueConfiguration.RoutingKey);

                foreach (var routeKeyList in queueConfiguration.RoutingKeys)
                {
                    await channel.QueueBindAsync(queue: queueConfiguration.QueueName, exchange: queueConfiguration.ExchangeName, routingKey: routeKeyList);
                }

                if (queueConfiguration.CreateRetryQueue)
                    await channel.CreateRetryQueueAsync(queueConfiguration.ExchangeName, queueConfiguration.RoutingKey, queueConfiguration.QueueName, queueConfiguration.SecondsToRetry);

                logger.LogInformation("Successfully created channel.");

                return channel;
            }
            catch (QueueDeadInvalidConfigurationException ex)
            {
                if (reconnect)
                    throw;

                logger.LogError(ex, "Invalid configuration in the dead queue. The queue will be updated.");

                return await CreateModelAsync(queueConfiguration, logger, reconnect: true, deleteQueueDead: true);
            }
            catch
            {
                if (reconnect)
                    throw;

                return await CreateModelAsync(queueConfiguration, logger, reconnect: true);
            }
        }

        private static readonly ConcurrentDictionary<string, IConnection> connections = new ConcurrentDictionary<string, IConnection>();

        private static async Task<IConnection> CreateConnectionAsync(QueueConfiguration queueConfiguration, ILogger logger, bool reconnect)
        {
            var nome = $"{queueConfiguration.HostName}-{queueConfiguration.Port}-{queueConfiguration.UserName}";

            if (connections.TryGetValue(nome, out var connection))
            {
                if (!reconnect)
                    return connection;

                connections.TryRemove(nome, out var antiga);
                try
                {
                    antiga.Dispose();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error dropping connection");
                }

                return await CreateConnectionAsync(queueConfiguration, logger, false);
            }

            var nameMachine = string.Empty;

            var factory = new ConnectionFactory()
            {
                HostName = queueConfiguration.HostName,
                Port = queueConfiguration.Port,
                UserName = queueConfiguration.UserName,
                Password = queueConfiguration.Password,
                VirtualHost = queueConfiguration.VirtualHost,
                ClientProperties = GetProperties(queueConfiguration, logger, out nameMachine),
                ClientProvidedName = nameMachine
            };

            connection = await factory.CreateConnectionAsync();
            connection.ConnectionShutdownAsync += (sender, ea) =>
            {
                logger.LogError("Connection shutdown: {@ShutdownEventArgs}", ea);

                return Task.CompletedTask;
            };

            connections.TryAdd(nome, connection);

            return connection;
        }

        public static Dictionary<string, object> GetProperties(QueueConfiguration queueConfiguration, ILogger logger, out string nameMachine)
        {
            nameMachine = Environment.MachineName;

            string nameApp, folderApp;
            GetNameApp(logger, out nameApp, out folderApp);

            return new Dictionary<string, object>
            {
                {"client_api",  "DotNetBaseQueue.RabbitMQ"},
                { "platform", GetPlatform()},
                { "os", Environment.OSVersion.ToString()},
                { "version", GetVersionApp()},
                { "application", nameApp},
                { "application_location", folderApp},
                { "machine_name", nameMachine},
                { "user", queueConfiguration.UserName},
                { "connected", DateTime.UtcNow.ToString("u")}
            };
        }

        private static void GetNameApp(ILogger logger, out string nameApp, out string folderApp)
        {
            var pathApp = Environment.GetCommandLineArgs()[0];

            nameApp = "unknown";
            folderApp = "unknown";

            if (string.IsNullOrWhiteSpace(pathApp))
                return;

            try
            {
                nameApp = Path.GetFileName(pathApp);
                folderApp = Path.GetDirectoryName(pathApp) ?? "unknown";
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error when get app name");
            }
        }

        private static string GetVersionApp()
        {
            try
            {
                return Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "unknown";
            }
            catch
            {
                return "unknown";
            }
        }

        private static string GetPlatform()
        {
            var platform = RuntimeInformation.FrameworkDescription;
            var frameworkName = Assembly.GetEntryAssembly()?.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName;
            if (frameworkName != null)
                platform = $"{platform} [{frameworkName}]";

            return platform;
        }
    }
}
