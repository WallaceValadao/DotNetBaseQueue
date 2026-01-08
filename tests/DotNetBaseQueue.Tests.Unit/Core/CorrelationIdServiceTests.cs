using DotNetBaseQueue.RabbitMQ.Core.Logs;
using FluentAssertions;

namespace DotNetBaseQueue.Tests.Unit.Core;

public class CorrelationIdServiceTests
{
    [Fact]
    public void Constructor_ShouldGenerateValidCorrelationId()
    {
        var service = new CorrelationIdService();
        var correlationId = service.Get();

        correlationId.Should().NotBeNullOrEmpty();
        correlationId.Should().HaveLength(32);
        correlationId.Should().MatchRegex("^[a-fA-F0-9]{32}$");
    }

    [Fact]
    public void Get_ShouldReturnSameCorrelationId()
    {
        var service = new CorrelationIdService();
        var firstCall = service.Get();

        var secondCall = service.Get();

        firstCall.Should().Be(secondCall);
    }

    [Fact]
    public void Set_ShouldUpdateCorrelationId()
    {
        // Arrange
        var service = new CorrelationIdService();
        var originalId = service.Get();
        var newId = "abc123def456";

        // Act
        service.Set(newId);
        var updatedId = service.Get();

        // Assert
        updatedId.Should().Be("abc123def456");
        updatedId.Should().NotBe(originalId);
    }

    [Fact]
    public void Set_ShouldRemoveHyphens()
    {
        // Arrange
        var service = new CorrelationIdService();
        var idWithHyphens = "abc-123-def-456";

        // Act
        service.Set(idWithHyphens);
        var result = service.Get();

        // Assert
        result.Should().Be("abc123def456");
        result.Should().NotContain("-");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Set_WithEmptyOrWhitespace_ShouldStillWork(string value)
    {
        // Arrange
        var service = new CorrelationIdService();

        // Act
        service.Set(value);
        var result = service.Get();

        // Assert
        result.Should().Be(value.Replace("-", string.Empty));
    }
}
