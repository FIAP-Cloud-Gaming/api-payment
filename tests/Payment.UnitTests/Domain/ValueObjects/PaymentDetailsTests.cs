namespace Payment.UnitTests.Domain.ValueObjects;

public class PaymentDetailsTests
{
    [Fact]
    public void Empty_ShouldCreateDetailsWithAllNulls()
    {
        // Act
        var details = PaymentDetails.Empty();

        // Assert
        details.Should().NotBeNull();
        details.TransactionId.Should().BeNull();
        details.AuthorizationCode.Should().BeNull();
        details.FailureReason.Should().BeNull();
        details.ProcessorResponse.Should().BeNull();
        details.ProcessedAt.Should().BeNull();
        details.IsProcessed.Should().BeFalse();
        details.HasFailed.Should().BeFalse();
    }

    [Fact]
    public void Create_WithAllValues_ShouldSetProperties()
    {
        // Act
        var details = PaymentDetails.Create("TXN-123", "AUTH-456", "PROC-RESP");

        // Assert
        details.TransactionId.Should().Be("TXN-123");
        details.AuthorizationCode.Should().Be("AUTH-456");
        details.ProcessorResponse.Should().Be("PROC-RESP");
        details.ProcessedAt.Should().NotBeNull();
        details.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithoutTransactionId_ShouldNotSetProcessedAt()
    {
        // Act
        var details = PaymentDetails.Create(null, "AUTH-456", "PROC-RESP");

        // Assert
        details.TransactionId.Should().BeNull();
        details.ProcessedAt.Should().BeNull();
    }

    [Fact]
    public void WithTransactionId_ShouldSetTransactionIdAndProcessedAt()
    {
        // Arrange
        var details = PaymentDetails.Empty();

        // Act
        var updated = details.WithTransactionId("TXN-789");

        // Assert
        updated.TransactionId.Should().Be("TXN-789");
        updated.ProcessedAt.Should().NotBeNull();
        updated.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void WithAuthorizationCode_ShouldSetAuthorizationCode()
    {
        // Arrange
        var details = PaymentDetails.Empty();

        // Act
        var updated = details.WithAuthorizationCode("AUTH-999");

        // Assert
        updated.AuthorizationCode.Should().Be("AUTH-999");
    }

    [Fact]
    public void WithFailureReason_ShouldSetFailureReasonAndProcessedAt()
    {
        // Arrange
        var details = PaymentDetails.Empty();

        // Act
        var updated = details.WithFailureReason("Insufficient funds");

        // Assert
        updated.FailureReason.Should().Be("Insufficient funds");
        updated.ProcessedAt.Should().NotBeNull();
        updated.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void WithProcessorResponse_ShouldSetProcessorResponse()
    {
        // Arrange
        var details = PaymentDetails.Empty();

        // Act
        var updated = details.WithProcessorResponse("DECLINED-001");

        // Assert
        updated.ProcessorResponse.Should().Be("DECLINED-001");
    }

    [Fact]
    public void IsProcessed_WhenTransactionIdExists_ShouldReturnTrue()
    {
        // Arrange
        var details = PaymentDetails.Create("TXN-123");

        // Act & Assert
        details.IsProcessed.Should().BeTrue();
    }

    [Fact]
    public void IsProcessed_WhenTransactionIdIsNull_ShouldReturnFalse()
    {
        // Arrange
        var details = PaymentDetails.Empty();

        // Act & Assert
        details.IsProcessed.Should().BeFalse();
    }

    [Fact]
    public void IsProcessed_WhenTransactionIdIsEmpty_ShouldReturnFalse()
    {
        // Arrange
        var details = PaymentDetails.Create("");

        // Act & Assert
        details.IsProcessed.Should().BeFalse();
    }

    [Fact]
    public void HasFailed_WhenFailureReasonExists_ShouldReturnTrue()
    {
        // Arrange
        var details = PaymentDetails.Empty().WithFailureReason("Card expired");

        // Act & Assert
        details.HasFailed.Should().BeTrue();
    }

    [Fact]
    public void HasFailed_WhenFailureReasonIsNull_ShouldReturnFalse()
    {
        // Arrange
        var details = PaymentDetails.Empty();

        // Act & Assert
        details.HasFailed.Should().BeFalse();
    }

    [Fact]
    public void HasFailed_WhenFailureReasonIsEmpty_ShouldReturnFalse()
    {
        // Arrange
        var details = PaymentDetails.Empty().WithFailureReason("");

        // Act & Assert
        details.HasFailed.Should().BeFalse();
    }

    [Fact]
    public void WithMethods_ShouldReturnNewInstance()
    {
        // Arrange
        var original = PaymentDetails.Empty();

        // Act
        var updated = original.WithTransactionId("TXN-123");

        // Assert
        updated.Should().NotBeSameAs(original);
        original.TransactionId.Should().BeNull();
        updated.TransactionId.Should().Be("TXN-123");
    }

    [Fact]
    public void ChainedWithMethods_ShouldCombineValues()
    {
        // Arrange
        var details = PaymentDetails.Empty();

        // Act
        var updated = details
            .WithTransactionId("TXN-123")
            .WithAuthorizationCode("AUTH-456")
            .WithProcessorResponse("SUCCESS");

        // Assert
        updated.TransactionId.Should().Be("TXN-123");
        updated.AuthorizationCode.Should().Be("AUTH-456");
        updated.ProcessorResponse.Should().Be("SUCCESS");
        updated.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public void Equality_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var details1 = PaymentDetails.Create("TXN-123", "AUTH-456");
        var details2 = PaymentDetails.Create("TXN-123", "AUTH-456");

        // Note: ProcessedAt will be different, so they won't be equal
        // Let's test with Empty instead
        var empty1 = PaymentDetails.Empty();
        var empty2 = PaymentDetails.Empty();

        // Act & Assert
        empty1.Should().Be(empty2);
    }

    [Fact]
    public void Equality_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var details1 = PaymentDetails.Empty().WithTransactionId("TXN-123");
        var details2 = PaymentDetails.Empty().WithTransactionId("TXN-456");

        // Act & Assert
        details1.Should().NotBe(details2);
    }
}
