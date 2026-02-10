namespace Payment.UnitTests.Domain;

public class PaymentIdTests
{
    [Fact]
    public void NewId_ShouldCreateNonEmptyGuid()
    {
        // Act
        var paymentId = PaymentId.NewId();

        // Assert
        paymentId.Should().NotBeNull();
        paymentId.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void NewId_ShouldCreateUniqueIds()
    {
        // Act
        var id1 = PaymentId.NewId();
        var id2 = PaymentId.NewId();

        // Assert
        id1.Should().NotBe(id2);
        id1.Value.Should().NotBe(id2.Value);
    }

    [Fact]
    public void FromGuid_WithValidGuid_ShouldCreatePaymentId()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var paymentId = PaymentId.From(guid);

        // Assert
        paymentId.Should().NotBeNull();
        paymentId.Value.Should().Be(guid);
    }

    [Fact]
    public void FromGuid_WithEmptyGuid_ShouldThrowException()
    {
        // Act
        var act = () => PaymentId.From(Guid.Empty);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("PaymentId cannot be empty*")
            .And.ParamName.Should().Be("value");
    }

    [Fact]
    public void FromString_WithValidGuidString_ShouldCreatePaymentId()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var guidString = guid.ToString();

        // Act
        var paymentId = PaymentId.From(guidString);

        // Assert
        paymentId.Should().NotBeNull();
        paymentId.Value.Should().Be(guid);
    }

    [Fact]
    public void FromString_WithInvalidString_ShouldThrowException()
    {
        // Act
        var act = () => PaymentId.From("invalid-guid");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Invalid PaymentId format: invalid-guid*")
            .And.ParamName.Should().Be("value");
    }

    [Fact]
    public void FromString_WithEmptyString_ShouldThrowException()
    {
        // Act
        var act = () => PaymentId.From("");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Invalid PaymentId format:*")
            .And.ParamName.Should().Be("value");
    }

    [Fact]
    public void ToString_ShouldReturnGuidString()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var paymentId = PaymentId.From(guid);

        // Act
        var result = paymentId.ToString();

        // Assert
        result.Should().Be(guid.ToString());
    }

    [Fact]
    public void ImplicitConversion_ToGuid_ShouldWork()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var paymentId = PaymentId.From(guid);

        // Act
        Guid convertedGuid = paymentId;

        // Assert
        convertedGuid.Should().Be(guid);
    }

    [Fact]
    public void Equality_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id1 = PaymentId.From(guid);
        var id2 = PaymentId.From(guid);

        // Act & Assert
        id1.Should().Be(id2);
        (id1 == id2).Should().BeTrue();
    }

    [Fact]
    public void Equality_WithDifferentValue_ShouldNotBeEqual()
    {
        // Arrange
        var id1 = PaymentId.NewId();
        var id2 = PaymentId.NewId();

        // Act & Assert
        id1.Should().NotBe(id2);
        (id1 != id2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id1 = PaymentId.From(guid);
        var id2 = PaymentId.From(guid);

        // Act & Assert
        id1.GetHashCode().Should().Be(id2.GetHashCode());
    }
}
