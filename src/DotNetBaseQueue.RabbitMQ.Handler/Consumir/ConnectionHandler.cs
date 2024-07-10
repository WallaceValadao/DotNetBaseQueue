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

namespace DotNetBaseQueue.RabbitMQ.Handler.Consumir
{
    internal static class ConnectionHandler
    {
        internal static IModel CreateConnection(RabbitConfiguration rabbitMQConfiguration, ILogger logger)
        {
            return CreateModel(rabbitMQConfiguration, logger);
        }

        internal static IModel CreateModel(RabbitConfiguration rabbitMQConfiguration, ILogger logger, bool reconnect = false, bool deleteQueueDead = false)
        {
            try
            {
                var connection = CreateConnection(rabbitMQConfiguration, logger, reconnect);
                var channel = connection.CreateModel();
                channel.ExchangeDeclare(exchange: rabbitMQConfiguration.ExchangeName, type: rabbitMQConfiguration.ExchangeType, durable: true);

                channel.ModelShutdown += (sender, ea) =>
                {
                    logger.LogError($"Channel error: {ea}");
                };

                Dictionary<string, object> args = null;
                if (rabbitMQConfiguration.CreateDeadLetterQueue)
                    args = channel.CreateDeadLetterQueue(rabbitMQConfiguration.ExchangeName, rabbitMQConfiguration.QueueName, deleteQueueDead);

                if (rabbitMQConfiguration.CreateRetryQueue)
                    channel.CreateRetryQueue(rabbitMQConfiguration.ExchangeName, rabbitMQConfiguration.RoutingKey, rabbitMQConfiguration.QueueName, rabbitMQConfiguration.SecondsToRetry);

                channel.QueueDeclare(queue: rabbitMQConfiguration.QueueName, durable: true, exclusive: false, autoDelete: false, arguments: args);
                channel.QueueBind(queue: rabbitMQConfiguration.QueueName, exchange: rabbitMQConfiguration.ExchangeName, routingKey: rabbitMQConfiguration.RoutingKey);

                logger.LogInformation("Successfully created channel.");

                return channel;
            }
            catch (QueueDeadInvalidConfigurationException ex)
            {
                if (reconnect)
                    throw;
                    
                logger.LogError(ex, "Invalid configuration in the dead queue. The queue will be updated.");

                return CreateModel(rabbitMQConfiguration, logger, reconnect: true, deleteQueueDead: true);
            }
            catch
            {
                if (reconnect)
                    throw;

                return CreateModel(rabbitMQConfiguration, logger, reconnect: true);
            }
        }

        private static readonly ConcurrentDictionary<string, IConnection> connections = new ConcurrentDictionary<string, IConnection>();

        private static IConnection CreateConnection(RabbitConfiguration rabbitMQConfiguration, ILogger logger, bool reconnect)
        {
            var nome = $"{rabbitMQConfiguration.HostName}-{rabbitMQConfiguration.Port}-{rabbitMQConfiguration.UserName}";

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

                return CreateConnection(rabbitMQConfiguration, logger, false);
            }

            var nameMachine = string.Empty;

            var factory = new ConnectionFactory()
            {
                HostName = rabbitMQConfiguration.HostName,
                Port = rabbitMQConfiguration.Port,
                UserName = rabbitMQConfiguration.UserName,
                Password = rabbitMQConfiguration.Password,
                VirtualHost = rabbitMQConfiguration.VirtualHost,
                ClientProperties = GetProperties(rabbitMQConfiguration, logger, out nameMachine),
                ClientProvidedName = nameMachine
            };

            connection = factory.CreateConnection();
            connection.ConnectionShutdown += (sender, ea) =>
            {
                logger.LogError($"Connection error: {ea}");
            };

            connections.TryAdd(nome, connection);

            return connection;
        }


        public static Dictionary<string, object> GetProperties(RabbitConfiguration rabbitMQConfiguration, ILogger logger, out string nameMachine)
        {
            nameMachine = Environment.MachineName;

            string nameApp, folderApp;
            GetNameApp(logger, out nameApp, out folderApp);

            return new Dictionary<string, object>
            {
                {"client_api",  "RabbitMqExtension"},
                { "product", nameApp},
                { "platform", GetPlatform()},
                { "os", Environment.OSVersion.ToString()},
                { "version", GetVersionApp()},
                { "application", nameApp},
                { "application_location", folderApp},
                { "machine_name", nameMachine},
                { "user", rabbitMQConfiguration.UserName},
                { "connected", DateTime.UtcNow.ToString("u")}
            };
        }

        private static void GetNameApp(ILogger logger, out string nomeAplicacao, out string pastaAplicacao)
        {
            var caminhoAplicacao = Environment.GetCommandLineArgs()[0];

            nomeAplicacao = "unknown";
            pastaAplicacao = "unknown";

            if (string.IsNullOrWhiteSpace(caminhoAplicacao))
                return;

            try
            {
                nomeAplicacao = Path.GetFileName(caminhoAplicacao);
                pastaAplicacao = Path.GetDirectoryName(caminhoAplicacao) ?? "unknown";
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
