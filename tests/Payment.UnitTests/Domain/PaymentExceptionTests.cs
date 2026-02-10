namespace Payment.UnitTests.Domain;

public class PaymentExceptionTests
{
    [Fact]
    public void DefaultConstructor_ShouldCreateException()
    {
        // Act
        var exception = new PaymentException();

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void MessageConstructor_ShouldCreateExceptionWithMessage()
    {
        // Arrange
        var message = "Payment processing failed";

        // Act
        var exception = new PaymentException(message);

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void MessageAndInnerExceptionConstructor_ShouldCreateExceptionWithBoth()
    {
        // Arrange
        var message = "Payment processing failed";
        var innerException = new InvalidOperationException("Inner error");

        // Act
        var exception = new PaymentException(message, innerException);

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Be(message);
        exception.InnerException.Should().Be(innerException);
        exception.InnerException.Message.Should().Be("Inner error");
    }

    [Fact]
    public void PaymentException_ShouldBeOfTypeException()
    {
        // Act
        var exception = new PaymentException("Test");

        // Assert
        exception.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void PaymentException_ShouldBeThrowable()
    {
        // Act
        Action act = () => throw new PaymentException("Test error");

        // Assert
        act.Should().Throw<PaymentException>()
            .WithMessage("Test error");
    }

    [Fact]
    public void PaymentException_WithInnerException_ShouldBeThrowable()
    {
        // Arrange
        var innerException = new ArgumentException("Inner");

        // Act
        Action act = () => throw new PaymentException("Outer error", innerException);

        // Assert
        act.Should().Throw<PaymentException>()
            .WithMessage("Outer error")
            .WithInnerException<ArgumentException>()
            .WithMessage("Inner");
    }
}
