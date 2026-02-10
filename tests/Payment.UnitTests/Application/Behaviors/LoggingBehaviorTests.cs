namespace Payment.UnitTests.Application.Behaviors;

public sealed class LoggingBehaviorTests
{
    private readonly ILogger<LoggingBehavior<TestRequest, TestResponse>> _loggerWithResponse = NullLogger<LoggingBehavior<TestRequest, TestResponse>>.Instance;
    private readonly ILogger<LoggingBehavior<TestRequest>> _loggerSingleParam = NullLogger<LoggingBehavior<TestRequest>>.Instance;

    [Fact]
    public async Task Handle_ShouldCallNext()
    {
        // Arrange
        var behavior = new LoggingBehavior<TestRequest, TestResponse>(_loggerWithResponse);
        var request = new TestRequest();
        var expectedResponse = new TestResponse();
        var nextCalled = false;

        RequestHandlerDelegate<TestResponse> next = _ =>
        {
            nextCalled = true;
            return Task.FromResult(expectedResponse);
        };

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeTrue();
        result.Should().Be(expectedResponse);
    }

    [Fact]
    public async Task Handle_WhenNextThrows_ShouldRethrow()
    {
        // Arrange
        var behavior = new LoggingBehavior<TestRequest, TestResponse>(_loggerWithResponse);
        var request = new TestRequest();
        var expectedException = new InvalidOperationException("Test exception");

        RequestHandlerDelegate<TestResponse> next = _ => throw expectedException;

        // Act
        var act = async () => await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Test exception");
    }


    [Fact]
    public async Task Handle_SingleParam_ShouldCallNext()
    {
        // Arrange
        var behavior = new LoggingBehavior<TestRequest>(_loggerSingleParam);
        var request = new TestRequest();
        var nextCalled = false;

        RequestHandlerDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        // Act
        await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_SingleParam_WhenNextThrows_ShouldRethrow()
    {
        // Arrange
        var behavior = new LoggingBehavior<TestRequest>(_loggerSingleParam);
        var request = new TestRequest();
        var expectedException = new InvalidOperationException("Test exception");

        RequestHandlerDelegate next = _ => throw expectedException;

        // Act
        var act = async () => await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Test exception");
    }

    public sealed record TestRequest;
    public sealed record TestResponse;
}
