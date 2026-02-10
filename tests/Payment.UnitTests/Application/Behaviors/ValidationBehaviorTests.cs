namespace Payment.UnitTests.Application.Behaviors;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_NoValidators_ShouldCallNext()
    {
        // Arrange
        var validators = Enumerable.Empty<IValidator<TestRequest>>();
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(validators);
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
    public async Task Handle_ValidRequest_ShouldCallNext()
    {
        // Arrange
        var validator = Substitute.For<IValidator<TestRequest>>();
        validator.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        var validators = new[] { validator };
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(validators);
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
    public async Task Handle_InvalidRequest_ShouldThrowValidationException()
    {
        // Arrange
        var validator = Substitute.For<IValidator<TestRequest>>();
        var validationFailures = new[]
        {
            new ValidationFailure("Property1", "Error message 1"),
            new ValidationFailure("Property2", "Error message 2")
        };
        validator.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(validationFailures));

        var validators = new[] { validator };
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(validators);
        var request = new TestRequest();

        RequestHandlerDelegate<TestResponse> next = _ => Task.FromResult(new TestResponse());

        // Act
        var act = async () => await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .Where(ex => ex.Errors.Count() == 2);
    }

    [Fact]
    public async Task Handle_MultipleValidators_ShouldAggregateErrors()
    {
        // Arrange
        var validator1 = Substitute.For<IValidator<TestRequest>>();
        validator1.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[] { new ValidationFailure("Property1", "Error from validator 1") }));

        var validator2 = Substitute.For<IValidator<TestRequest>>();
        validator2.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[] { new ValidationFailure("Property2", "Error from validator 2") }));

        var validators = new[] { validator1, validator2 };
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(validators);
        var request = new TestRequest();

        RequestHandlerDelegate<TestResponse> next = _ => Task.FromResult(new TestResponse());

        // Act
        var act = async () => await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .Where(ex => ex.Errors.Count() == 2);
    }

    [Fact]
    public async Task Handle_SingleParam_NoValidators_ShouldCallNext()
    {
        // Arrange
        var validators = Enumerable.Empty<IValidator<TestRequest>>();
        var behavior = new ValidationBehavior<TestRequest>(validators);
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
    public async Task Handle_SingleParam_InvalidRequest_ShouldThrowValidationException()
    {
        // Arrange
        var validator = Substitute.For<IValidator<TestRequest>>();
        var validationFailures = new[]
        {
            new ValidationFailure("Property1", "Error message 1")
        };
        validator.ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(validationFailures));

        var validators = new[] { validator };
        var behavior = new ValidationBehavior<TestRequest>(validators);
        var request = new TestRequest();

        RequestHandlerDelegate next = _ => Task.CompletedTask;

        // Act
        var act = async () => await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    public sealed record TestRequest;
    public sealed record TestResponse;
}
