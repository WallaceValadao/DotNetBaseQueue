using System.Text;
using System.Text.Json;
using DotNetBaseQueue.RabbitMQ.Core;
using FluentAssertions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DotNetBaseQueue.Tests.Unit.Core;

public class MessageHelperTests
{
    private class TestMessage
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    [Fact]
    public void GetEntityQueue_ValidJson_ShouldDeserializeCorrectly()
    {
        // Arrange
        var message = new TestMessage
        {
            Id = 123,
            Name = "Test Message",
            Timestamp = DateTime.UtcNow
        };
        var json = JsonSerializer.Serialize(message);
        var body = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(json));
        var args = CreateBasicDeliverEventArgs(body);

        // Act
        var result = args.GetEntityQueue<TestMessage>();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(123);
        result.Name.Should().Be("Test Message");
    }

    [Fact]
    public void GetEntityQueue_EmptyJson_ShouldReturnDefault()
    {
        // Arrange
        var body = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes("{}"));
        var args = CreateBasicDeliverEventArgs(body);

        // Act
        var result = args.GetEntityQueue<TestMessage>();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(0);
        result.Name.Should().BeNullOrEmpty();
    }

    [Fact]
    public void GetHabbitMqProperties_ShouldSetCorrelationId()
    {
        // Arrange
        var correlationId = "test-correlation-id-123";

        // Act
        var properties = MessageHelper.GetHabbitMqProperties(correlationId);

        // Assert
        properties.Should().NotBeNull();
        properties.Headers.Should().ContainKey(QueueConstraints.CORRELATION_ID_HEADER);
        var headerValue = Encoding.UTF8.GetString((byte[])properties.Headers[QueueConstraints.CORRELATION_ID_HEADER]);
        headerValue.Should().Be(correlationId);
    }

    [Fact]
    public void GetHabbitMqProperties_WithPersistent_ShouldSetDeliveryMode()
    {
        // Arrange
        var correlationId = "test-id";

        // Act
        var properties = MessageHelper.GetHabbitMqProperties(correlationId, persistent: true);

        // Assert
        properties.DeliveryMode.Should().Be(DeliveryModes.Persistent);
    }

    [Fact]
    public void GetHabbitMqProperties_WithoutPersistent_ShouldSetTransientDeliveryMode()
    {
        // Arrange
        var correlationId = "test-id";

        // Act
        var properties = MessageHelper.GetHabbitMqProperties(correlationId, persistent: false);

        // Assert
        properties.DeliveryMode.Should().Be(DeliveryModes.Transient);
    }

    private static BasicDeliverEventArgs CreateBasicDeliverEventArgs(ReadOnlyMemory<byte> body)
    {
        return new BasicDeliverEventArgs(
            consumerTag: "test-consumer",
            deliveryTag: 1,
            redelivered: false,
            exchange: "test-exchange",
            routingKey: "test-routing-key",
            properties: new BasicProperties(),
            body: body
        );
    }
}
