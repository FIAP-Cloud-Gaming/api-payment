using DomainPayment = Payment.Domain.AggregatesModel.PaymentAggregate.Payment;
using Payment.UnitTests.Helpers;

namespace Payment.UnitTests.Domain;

public class PaymentAggregateTests
{
    #region Create Tests

    [Fact]
    public void Create_WithValidPixPayment_ShouldCreateSuccessfully()
    {
        // Arrange
        var orderId = "ORDER-001";
        var userId = "user-123";
        var method = PaymentMethod.Pix;
        var items = PaymentTestFactory.CreateValidItems(1, 199.99m);

        // Act
        var payment = DomainPayment.Create(orderId, userId, method, items);

        // Assert
        payment.Should().NotBeNull();
        payment.Id.Should().NotBeNull();
        payment.OrderId.Should().Be(orderId);
        payment.UserId.Should().Be(userId);
        payment.Method.Should().Be(method);
        payment.Status.Should().Be(PaymentStatus.Pending);
        payment.TotalAmount.Value.Should().Be(199.99m);
        payment.TotalAmount.Currency.Should().Be("BRL");
        payment.Items.Should().HaveCount(1);
        payment.CardInfo.Should().BeNull();
        payment.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        payment.Notifications.Should().ContainSingle()
            .Which.Should().BeOfType<PaymentStartedDomainEvent>();
    }

    [Fact]
    public void Create_WithValidCreditCardPayment_ShouldCreateSuccessfully()
    {
        // Arrange
        var orderId = "ORDER-002";
        var userId = "user-456";
        var method = PaymentMethod.CreditCard;
        var items = PaymentTestFactory.CreateValidItems(1, 299.99m);
        var cardInfo = CardInfo.Create("Jane Doe", "5555555555554444", "Mastercard", 6, DateTime.UtcNow.Year + 3);

        // Act
        var payment = DomainPayment.Create(orderId, userId, method, items, cardInfo);

        // Assert
        payment.Should().NotBeNull();
        payment.Method.Should().Be(PaymentMethod.CreditCard);
        payment.CardInfo.Should().NotBeNull();
        payment.CardInfo!.CardHolderName.Should().Be("Jane Doe");
        payment.CardInfo.MaskedCardNumber.Should().Be("**** **** **** 4444");
        payment.CardInfo.CardBrand.Should().Be("Mastercard");
    }

    [Fact]
    public void Create_WithValidDebitCardPayment_ShouldCreateSuccessfully()
    {
        // Arrange
        var orderId = "ORDER-003";
        var userId = "user-789";
        var method = PaymentMethod.DebitCard;
        var items = PaymentTestFactory.CreateValidItems(1, 149.99m);
        var cardInfo = CardInfo.Create("Bob Smith", "378282246310005", "Amex", 3, DateTime.UtcNow.Year + 1);

        // Act
        var payment = DomainPayment.Create(orderId, userId, method, items, cardInfo);

        // Assert
        payment.Should().NotBeNull();
        payment.Method.Should().Be(PaymentMethod.DebitCard);
        payment.CardInfo.Should().NotBeNull();
    }

    [Fact]
    public void Create_WithMissingOrderId_ShouldThrowException()
    {
        // Arrange
        var items = PaymentTestFactory.CreateValidItems(1);

        // Act
        var act = () => DomainPayment.Create("", "user-123", PaymentMethod.Pix, items);

        // Assert
        act.Should().Throw<PaymentException>()
            .WithMessage("Order ID is required");
    }

    [Fact]
    public void Create_WithMissingUserId_ShouldThrowException()
    {
        // Arrange
        var items = PaymentTestFactory.CreateValidItems(1);

        // Act
        var act = () => DomainPayment.Create("ORDER-001", "", PaymentMethod.Pix, items);

        // Assert
        act.Should().Throw<PaymentException>()
            .WithMessage("User ID is required");
    }

    [Fact]
    public void Create_WithEmptyItems_ShouldThrowException()
    {
        // Arrange
        var emptyItems = new List<PaymentItem>();

        // Act
        var act = () => DomainPayment.Create("ORDER-001", "user-123", PaymentMethod.Pix, emptyItems);

        // Assert
        act.Should().Throw<PaymentException>()
            .WithMessage("Payment must have at least one item");
    }

    [Fact]
    public void Create_WithNullItems_ShouldThrowException()
    {
        // Act
        var act = () => DomainPayment.Create("ORDER-001", "user-123", PaymentMethod.Pix, null!);

        // Assert
        act.Should().Throw<PaymentException>()
            .WithMessage("Payment must have at least one item");
    }

    [Fact]
    public void Create_WithCreditCardWithoutCardInfo_ShouldThrowException()
    {
        // Arrange
        var items = PaymentTestFactory.CreateValidItems(1);

        // Act
        var act = () => DomainPayment.Create("ORDER-001", "user-123", PaymentMethod.CreditCard, items);

        // Assert
        act.Should().Throw<PaymentException>()
            .WithMessage("CreditCard requires card information");
    }

    [Fact]
    public void Create_WithExpiredCard_ShouldThrowException()
    {
        // Arrange
        var items = PaymentTestFactory.CreateValidItems(1);
        var now = DateTime.UtcNow;

        // Create a card that expires in a past month of current year (if we're not in January)
        // Otherwise skip this test as we can't create an expired card due to CardInfo validation
        if (now.Month == 1)
        {
            // Cannot create past year card, so skip this scenario
            // The CardInfo constructor itself prevents creating past year cards
            return;
        }

        var expiredCard = CardInfo.Create("John Doe", "4111111111111111", "Visa", now.Month - 1, now.Year);

        // Act
        var act = () => DomainPayment.Create("ORDER-001", "user-123", PaymentMethod.CreditCard, items, expiredCard);

        // Assert
        act.Should().Throw<PaymentException>()
            .WithMessage("Card is expired");
    }

    [Fact]
    public void Create_WithMultipleItems_ShouldCalculateTotalCorrectly()
    {
        // Arrange
        var items = new List<PaymentItem>
        {
            new("product-1", "Product 1", 2, Money.FromDecimal(50.00m, "BRL")),
            new("product-2", "Product 2", 1, Money.FromDecimal(100.00m, "BRL")),
            new("product-3", "Product 3", 3, Money.FromDecimal(25.00m, "BRL"))
        };

        // Act
        var payment = DomainPayment.Create("ORDER-001", "user-123", PaymentMethod.Pix, items);

        // Assert
        payment.TotalAmount.Value.Should().Be(275.00m); // (2*50) + (1*100) + (3*25) = 100 + 100 + 75 = 275
        payment.Items.Should().HaveCount(3);
    }

    #endregion

    #region StartProcessing Tests

    [Fact]
    public void StartProcessing_FromPending_ShouldSucceed()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateValidPayment();

        // Act
        payment.StartProcessing();

        // Assert
        payment.Status.Should().Be(PaymentStatus.Processing);
        payment.UpdatedAt.Should().NotBeNull();
        payment.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void StartProcessing_FromProcessing_ShouldThrowException()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateProcessingPayment();

        // Act
        var act = () => payment.StartProcessing();

        // Assert
        act.Should().Throw<PaymentException>()
            .WithMessage("Cannot start processing payment in Processing status");
    }

    [Fact]
    public void StartProcessing_FromSucceeded_ShouldThrowException()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateSucceededPayment();

        // Act
        var act = () => payment.StartProcessing();

        // Assert
        act.Should().Throw<PaymentException>()
            .WithMessage("Cannot start processing payment in Succeeded status");
    }

    [Fact]
    public void StartProcessing_FromFailed_ShouldThrowException()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateFailedPayment();

        // Act
        var act = () => payment.StartProcessing();

        // Assert
        act.Should().Throw<PaymentException>()
            .WithMessage("Cannot start processing payment in Failed status");
    }

    [Fact]
    public void StartProcessing_FromCancelled_ShouldThrowException()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateCancelledPayment();

        // Act
        var act = () => payment.StartProcessing();

        // Assert
        act.Should().Throw<PaymentException>()
            .WithMessage("Cannot start processing payment in Cancelled status");
    }

    [Fact]
    public void StartProcessing_FromRefunded_ShouldThrowException()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateRefundedPayment();

        // Act
        var act = () => payment.StartProcessing();

        // Assert
        act.Should().Throw<PaymentException>()
            .WithMessage("Cannot start processing payment in Refunded status");
    }

    #endregion

    #region MarkAsSucceeded Tests

    [Fact]
    public void MarkAsSucceeded_FromProcessing_ShouldSucceed()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateProcessingPayment();

        // Act
        payment.MarkAsSucceeded("TXN-12345", "AUTH-67890");

        // Assert
        payment.Status.Should().Be(PaymentStatus.Succeeded);
        payment.Details.TransactionId.Should().Be("TXN-12345");
        payment.Details.AuthorizationCode.Should().Be("AUTH-67890");
        payment.ProcessedAt.Should().NotBeNull();
        payment.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        payment.UpdatedAt.Should().NotBeNull();
        payment.Notifications.Should().ContainSingle()
            .Which.Should().BeOfType<PaymentSucceededDomainEvent>();
    }

    [Fact]
    public void MarkAsSucceeded_FromPending_ShouldSucceed()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateValidPayment();

        // Act
        payment.MarkAsSucceeded("TXN-12345");

        // Assert
        payment.Status.Should().Be(PaymentStatus.Succeeded);
        payment.Details.TransactionId.Should().Be("TXN-12345");
    }

    [Fact]
    public void MarkAsSucceeded_WithoutAuthorizationCode_ShouldSetEmptyString()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateProcessingPayment();

        // Act
        payment.MarkAsSucceeded("TXN-12345");

        // Assert
        payment.Status.Should().Be(PaymentStatus.Succeeded);
        payment.Details.TransactionId.Should().Be("TXN-12345");
        payment.Details.AuthorizationCode.Should().Be(string.Empty);
    }

    [Fact]
    public void MarkAsSucceeded_WithEmptyTransactionId_ShouldThrowException()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateProcessingPayment();

        // Act
        var act = () => payment.MarkAsSucceeded("");

        // Assert
        act.Should().Throw<PaymentException>()
            .WithMessage("Transaction ID is required");
    }

    [Fact]
    public void MarkAsSucceeded_FromSucceeded_ShouldThrowException()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateSucceededPayment();

        // Act
        var act = () => payment.MarkAsSucceeded("TXN-NEW");

        // Assert
        act.Should().Throw<PaymentException>()
            .WithMessage("Cannot mark payment as succeeded from Succeeded status");
    }

    [Fact]
    public void MarkAsSucceeded_FromFailed_ShouldThrowException()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateFailedPayment();

        // Act
        var act = () => payment.MarkAsSucceeded("TXN-12345");

        // Assert
        act.Should().Throw<PaymentException>()
            .WithMessage("Cannot mark payment as succeeded from Failed status");
    }

    [Fact]
    public void MarkAsSucceeded_RaisesPaymentSucceededDomainEvent()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateProcessingPayment();

        // Act
        payment.MarkAsSucceeded("TXN-12345", "AUTH-67890");

        // Assert
        var domainEvent = payment.Notifications.Should().ContainSingle()
            .Which.Should().BeOfType<PaymentSucceededDomainEvent>().Subject;
        domainEvent.Payment.Should().Be(payment);
    }

    #endregion

    #region MarkAsFailed Tests

    [Fact]
    public void MarkAsFailed_FromProcessing_ShouldSucceed()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateProcessingPayment();

        // Act
        payment.MarkAsFailed("Insufficient funds", "PROCESSOR-ERROR-001");

        // Assert
        payment.Status.Should().Be(PaymentStatus.Failed);
        payment.Details.FailureReason.Should().Be("Insufficient funds");
        payment.Details.ProcessorResponse.Should().Be("PROCESSOR-ERROR-001");
        payment.ProcessedAt.Should().NotBeNull();
        payment.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        payment.UpdatedAt.Should().NotBeNull();
        payment.Notifications.Should().ContainSingle()
            .Which.Should().BeOfType<PaymentFailedDomainEvent>();
    }

    [Fact]
    public void MarkAsFailed_FromPending_ShouldSucceed()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateValidPayment();

        // Act
        payment.MarkAsFailed("Card declined");

        // Assert
        payment.Status.Should().Be(PaymentStatus.Failed);
        payment.Details.FailureReason.Should().Be("Card declined");
    }

    [Fact]
    public void MarkAsFailed_WithoutProcessorResponse_ShouldSetEmptyString()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateProcessingPayment();

        // Act
        payment.MarkAsFailed("Network timeout");

        // Assert
        payment.Status.Should().Be(PaymentStatus.Failed);
        payment.Details.ProcessorResponse.Should().Be(string.Empty);
    }

    [Fact]
    public void MarkAsFailed_WithEmptyReason_ShouldThrowException()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateProcessingPayment();

        // Act
        var act = () => payment.MarkAsFailed("");

        // Assert
        act.Should().Throw<PaymentException>()
            .WithMessage("Failure reason is required");
    }

    [Fact]
    public void MarkAsFailed_FromSucceeded_ShouldThrowException()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateSucceededPayment();

        // Act
        var act = () => payment.MarkAsFailed("Some reason");

        // Assert
        act.Should().Throw<PaymentException>()
            .WithMessage("Cannot mark payment as failed from Succeeded status");
    }

    [Fact]
    public void MarkAsFailed_FromFailed_ShouldThrowException()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateFailedPayment();

        // Act
        var act = () => payment.MarkAsFailed("Another reason");

        // Assert
        act.Should().Throw<PaymentException>()
            .WithMessage("Cannot mark payment as failed from Failed status");
    }

    [Fact]
    public void MarkAsFailed_FromCancelled_ShouldThrowException()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateCancelledPayment();

        // Act
        var act = () => payment.MarkAsFailed("Some reason");

        // Assert
        act.Should().Throw<PaymentException>()
            .WithMessage("Cannot mark payment as failed from Cancelled status");
    }

    [Fact]
    public void MarkAsFailed_FromRefunded_ShouldThrowException()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateRefundedPayment();

        // Act
        var act = () => payment.MarkAsFailed("Some reason");

        // Assert
        act.Should().Throw<PaymentException>()
            .WithMessage("Cannot mark payment as failed from Refunded status");
    }

    [Fact]
    public void MarkAsFailed_RaisesPaymentFailedDomainEvent()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateProcessingPayment();

        // Act
        payment.MarkAsFailed("Card declined");

        // Assert
        var domainEvent = payment.Notifications.Should().ContainSingle()
            .Which.Should().BeOfType<PaymentFailedDomainEvent>().Subject;
        domainEvent.Payment.Should().Be(payment);
        domainEvent.FailureReason.Should().Be("Card declined");
    }

    #endregion

    #region Cancel Tests

    [Fact]
    public void Cancel_FromPending_ShouldSucceed()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateValidPayment();

        // Act
        payment.Cancel("User requested cancellation");

        // Assert
        payment.Status.Should().Be(PaymentStatus.Cancelled);
        payment.Details.FailureReason.Should().Be("Cancelled: User requested cancellation");
        payment.ProcessedAt.Should().NotBeNull();
        payment.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        payment.UpdatedAt.Should().NotBeNull();
        payment.Notifications.Should().ContainSingle()
            .Which.Should().BeOfType<PaymentCancelledDomainEvent>();
    }

    [Fact]
    public void Cancel_FromProcessing_ShouldSucceed()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateProcessingPayment();

        // Act
        payment.Cancel("Payment gateway timeout");

        // Assert
        payment.Status.Should().Be(PaymentStatus.Cancelled);
        payment.Details.FailureReason.Should().Be("Cancelled: Payment gateway timeout");
    }

    [Fact]
    public void Cancel_FromSucceeded_ShouldThrowException()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateSucceededPayment();

        // Act
        var act = () => payment.Cancel("Changed mind");

        // Assert
        act.Should().Throw<PaymentException>()
            .WithMessage("Cannot cancel a succeeded payment. Use refund instead.");
    }

    [Fact]
    public void Cancel_AlreadyCancelled_ShouldThrowException()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateCancelledPayment();

        // Act
        var act = () => payment.Cancel("Duplicate cancel");

        // Assert
        act.Should().Throw<PaymentException>()
            .WithMessage("Payment is already cancelled");
    }

    [Fact]
    public void Cancel_FromRefunded_ShouldThrowException()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateRefundedPayment();

        // Act
        var act = () => payment.Cancel("Some reason");

        // Assert
        act.Should().Throw<PaymentException>()
            .WithMessage("Cannot cancel a refunded payment");
    }

    [Fact]
    public void Cancel_RaisesPaymentCancelledDomainEvent()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateValidPayment();

        // Act
        payment.Cancel("User cancelled order");

        // Assert
        var domainEvent = payment.Notifications.Should().ContainSingle()
            .Which.Should().BeOfType<PaymentCancelledDomainEvent>().Subject;
        domainEvent.Payment.Should().Be(payment);
        domainEvent.Reason.Should().Be("User cancelled order");
    }

    [Fact]
    public void Cancel_SetsFailureReasonWithCancelledPrefix()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateValidPayment();

        // Act
        payment.Cancel("User action");

        // Assert
        payment.Details.FailureReason.Should().Be("Cancelled: User action");
    }

    #endregion

    #region Refund Tests

    [Fact]
    public void Refund_FromSucceeded_ShouldSucceed()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateSucceededPayment();

        // Act
        payment.Refund("Customer requested refund");

        // Assert
        payment.Status.Should().Be(PaymentStatus.Refunded);
        payment.UpdatedAt.Should().NotBeNull();
        payment.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        payment.Notifications.Should().ContainSingle()
            .Which.Should().BeOfType<PaymentRefundedDomainEvent>();
    }

    [Fact]
    public void Refund_WithEmptyReason_ShouldThrowException()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateSucceededPayment();

        // Act
        var act = () => payment.Refund("");

        // Assert
        act.Should().Throw<PaymentException>()
            .WithMessage("Refund reason is required");
    }

    [Fact]
    public void Refund_FromPending_ShouldThrowException()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateValidPayment();

        // Act
        var act = () => payment.Refund("Some reason");

        // Assert
        act.Should().Throw<PaymentException>()
            .WithMessage("Only succeeded payments can be refunded");
    }

    [Fact]
    public void Refund_FromProcessing_ShouldThrowException()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateProcessingPayment();

        // Act
        var act = () => payment.Refund("Some reason");

        // Assert
        act.Should().Throw<PaymentException>()
            .WithMessage("Only succeeded payments can be refunded");
    }

    [Fact]
    public void Refund_FromFailed_ShouldThrowException()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateFailedPayment();

        // Act
        var act = () => payment.Refund("Some reason");

        // Assert
        act.Should().Throw<PaymentException>()
            .WithMessage("Only succeeded payments can be refunded");
    }

    [Fact]
    public void Refund_FromCancelled_ShouldThrowException()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateCancelledPayment();

        // Act
        var act = () => payment.Refund("Some reason");

        // Assert
        act.Should().Throw<PaymentException>()
            .WithMessage("Only succeeded payments can be refunded");
    }

    [Fact]
    public void Refund_FromRefunded_ShouldThrowException()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateRefundedPayment();

        // Act
        var act = () => payment.Refund("Double refund");

        // Assert
        act.Should().Throw<PaymentException>()
            .WithMessage("Only succeeded payments can be refunded");
    }

    [Fact]
    public void Refund_RaisesPaymentRefundedDomainEvent()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateSucceededPayment();

        // Act
        payment.Refund("Defective product");

        // Assert
        var domainEvent = payment.Notifications.Should().ContainSingle()
            .Which.Should().BeOfType<PaymentRefundedDomainEvent>().Subject;
        domainEvent.Payment.Should().Be(payment);
        domainEvent.Reason.Should().Be("Defective product");
    }

    #endregion

    #region Helper Methods Tests

    [Fact]
    public void CanBeProcessed_WhenPending_ShouldReturnTrue()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateValidPayment();

        // Act & Assert
        payment.CanBeProcessed().Should().BeTrue();
    }

    [Fact]
    public void CanBeProcessed_WhenProcessing_ShouldReturnFalse()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateProcessingPayment();

        // Act & Assert
        payment.CanBeProcessed().Should().BeFalse();
    }

    [Fact]
    public void CanBeProcessed_WhenSucceeded_ShouldReturnFalse()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateSucceededPayment();

        // Act & Assert
        payment.CanBeProcessed().Should().BeFalse();
    }

    [Fact]
    public void CanBeCancelled_WhenPending_ShouldReturnTrue()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateValidPayment();

        // Act & Assert
        payment.CanBeCancelled().Should().BeTrue();
    }

    [Fact]
    public void CanBeCancelled_WhenProcessing_ShouldReturnTrue()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateProcessingPayment();

        // Act & Assert
        payment.CanBeCancelled().Should().BeTrue();
    }

    [Fact]
    public void CanBeCancelled_WhenSucceeded_ShouldReturnFalse()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateSucceededPayment();

        // Act & Assert
        payment.CanBeCancelled().Should().BeFalse();
    }

    [Fact]
    public void CanBeCancelled_WhenCancelled_ShouldReturnFalse()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateCancelledPayment();

        // Act & Assert
        payment.CanBeCancelled().Should().BeFalse();
    }

    [Fact]
    public void CanBeRefunded_WhenSucceeded_ShouldReturnTrue()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateSucceededPayment();

        // Act & Assert
        payment.CanBeRefunded().Should().BeTrue();
    }

    [Fact]
    public void CanBeRefunded_WhenPending_ShouldReturnFalse()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateValidPayment();

        // Act & Assert
        payment.CanBeRefunded().Should().BeFalse();
    }

    [Fact]
    public void CanBeRefunded_WhenRefunded_ShouldReturnFalse()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateRefundedPayment();

        // Act & Assert
        payment.CanBeRefunded().Should().BeFalse();
    }

    [Fact]
    public void IsInFinalState_WhenSucceeded_ShouldReturnTrue()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateSucceededPayment();

        // Act & Assert
        payment.IsInFinalState().Should().BeTrue();
    }

    [Fact]
    public void IsInFinalState_WhenFailed_ShouldReturnTrue()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateFailedPayment();

        // Act & Assert
        payment.IsInFinalState().Should().BeTrue();
    }

    [Fact]
    public void IsInFinalState_WhenCancelled_ShouldReturnTrue()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateCancelledPayment();

        // Act & Assert
        payment.IsInFinalState().Should().BeTrue();
    }

    [Fact]
    public void IsInFinalState_WhenRefunded_ShouldReturnTrue()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateRefundedPayment();

        // Act & Assert
        payment.IsInFinalState().Should().BeTrue();
    }

    [Fact]
    public void IsInFinalState_WhenPending_ShouldReturnFalse()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateValidPayment();

        // Act & Assert
        payment.IsInFinalState().Should().BeFalse();
    }

    [Fact]
    public void IsInFinalState_WhenProcessing_ShouldReturnFalse()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateProcessingPayment();

        // Act & Assert
        payment.IsInFinalState().Should().BeFalse();
    }

    #endregion

    #region Domain Events Tests

    [Fact]
    public void Create_ShouldRaisePaymentStartedDomainEvent()
    {
        // Arrange
        var items = PaymentTestFactory.CreateValidItems(1);

        // Act
        var payment = DomainPayment.Create("ORDER-001", "user-123", PaymentMethod.Pix, items);

        // Assert
        var domainEvent = payment.Notifications.Should().ContainSingle()
            .Which.Should().BeOfType<PaymentStartedDomainEvent>().Subject;
        domainEvent.Payment.Should().Be(payment);
    }

    #endregion
}
