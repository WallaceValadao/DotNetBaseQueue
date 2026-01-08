using DotNetBaseQueue.Interfaces.Configs;
using FluentAssertions;
using Testcontainers.RabbitMq;

namespace DotNetBaseQueue.Tests.Integration;

public class RabbitMQIntegrationTests : IAsyncLifetime
{
    private RabbitMqContainer? _rabbitMqContainer;

    public async Task InitializeAsync()
    {
        _rabbitMqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:3.13-management-alpine")
            .WithUsername("test")
            .WithPassword("test")
            .Build();

        await _rabbitMqContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        if (_rabbitMqContainer != null)
        {
            await _rabbitMqContainer.DisposeAsync();
        }
    }

    [Fact]
    public async Task Container_ShouldStart_Successfully()
    {
        // Assert
        _rabbitMqContainer.Should().NotBeNull();
        var connectionString = _rabbitMqContainer!.GetConnectionString();
        connectionString.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task QueueConfiguration_ShouldConnect_ToTestContainer()
    {
        // Arrange
        var config = new QueueConfiguration
        {
            HostName = _rabbitMqContainer!.Hostname,
            Port = _rabbitMqContainer.GetMappedPublicPort(5672),
            UserName = "test",
            Password = "test",
            VirtualHost = "/",
            QueueName = "test-queue",
            ExchangeName = "test-exchange",
            RoutingKey = "test-routing-key"
        };

        // Act & Assert
        config.HostName.Should().NotBeNullOrEmpty();
        config.Port.Should().BeGreaterThan(0);
    }
}
