using DomainPayment = Payment.Domain.AggregatesModel.PaymentAggregate.Payment;
using Payment.UnitTests.Helpers;

namespace Payment.UnitTests.Application.Models;

public sealed class PaymentDtoTests
{
    [Fact]
    public void FromPayment_ValidPayment_ShouldMapAllProperties()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateValidPayment(
            orderId: "ORDER-001",
            userId: "user-123",
            method: PaymentMethod.Pix,
            amount: 199.99m
        );

        // Act
        var dto = PaymentDto.FromPayment(payment);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(payment.Id.ToString());
        dto.OrderId.Should().Be(payment.OrderId);
        dto.UserId.Should().Be(payment.UserId);
        dto.TotalAmount.Should().Be(payment.TotalAmount.Value);
        dto.Currency.Should().Be(payment.TotalAmount.Currency);
        dto.Status.Should().Be(payment.Status);
        dto.Method.Should().Be(payment.Method);
        dto.Items.Should().HaveCount(payment.Items.Count);
        dto.CreatedAt.Should().Be(payment.CreatedAt);
    }

    [Fact]
    public void FromPayment_WithCardInfo_ShouldMapCardInfo()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateValidPaymentWithCard(
            orderId: "ORDER-001",
            userId: "user-123",
            method: PaymentMethod.CreditCard,
            amount: 299.99m
        );

        // Act
        var dto = PaymentDto.FromPayment(payment);

        // Assert
        dto.Should().NotBeNull();
        dto.CardInfo.Should().NotBeNull();
        dto.CardInfo!.CardHolderName.Should().Be(payment.CardInfo!.CardHolderName);
        dto.CardInfo.CardNumber.Should().Be(payment.CardInfo.MaskedCardNumber);
        dto.CardInfo.CardBrand.Should().Be(payment.CardInfo.CardBrand);
        dto.CardInfo.ExpirationMonth.Should().Be(payment.CardInfo.ExpirationMonth);
        dto.CardInfo.ExpirationYear.Should().Be(payment.CardInfo.ExpirationYear);
    }

    [Fact]
    public void FromPayment_WithoutCardInfo_ShouldHaveNullCardInfo()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateValidPayment(
            orderId: "ORDER-001",
            userId: "user-123",
            method: PaymentMethod.Pix,
            amount: 99.99m
        );

        // Act
        var dto = PaymentDto.FromPayment(payment);

        // Assert
        dto.Should().NotBeNull();
        dto.CardInfo.Should().BeNull();
    }

    [Fact]
    public void FromPayment_NullPayment_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => PaymentDto.FromPayment(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void FromPayment_SucceededPayment_ShouldMapTransactionDetails()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateSucceededPayment(
            orderId: "ORDER-001",
            userId: "user-123",
            amount: 149.99m
        );

        // Act
        var dto = PaymentDto.FromPayment(payment);

        // Assert
        dto.Should().NotBeNull();
        dto.Status.Should().Be(PaymentStatus.Succeeded);
        dto.TransactionId.Should().NotBeNullOrEmpty();
        dto.AuthorizationCode.Should().NotBeNullOrEmpty();
        dto.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public void FromPayment_CancelledPayment_ShouldMapFailureReason()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateCancelledPayment();

        // Act
        var dto = PaymentDto.FromPayment(payment);

        // Assert
        dto.Should().NotBeNull();
        dto.Status.Should().Be(PaymentStatus.Cancelled);
        dto.FailureReason.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void FromPayment_RefundedPayment_ShouldMapCorrectly()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateRefundedPayment();

        // Act
        var dto = PaymentDto.FromPayment(payment);

        // Assert
        dto.Should().NotBeNull();
        dto.Status.Should().Be(PaymentStatus.Refunded);
        dto.TransactionId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void FromPayment_WithMultipleItems_ShouldMapAllItems()
    {
        // Arrange
        var items = new List<PaymentItem>
        {
            new("product-1", "Product 1", 2, Money.FromDecimal(50.00m, "BRL")),
            new("product-2", "Product 2", 1, Money.FromDecimal(150.00m, "BRL"))
        };
        var payment = DomainPayment.Create("ORDER-001", "user-123", PaymentMethod.Pix, items);
        payment.ClearNotification();

        // Act
        var dto = PaymentDto.FromPayment(payment);

        // Assert
        dto.Should().NotBeNull();
        dto.Items.Should().HaveCount(2);
        dto.Items.Should().Contain(i => i.ProductId == "product-1" && i.Quantity == 2);
        dto.Items.Should().Contain(i => i.ProductId == "product-2" && i.Quantity == 1);
        dto.TotalAmount.Should().Be(250.00m);
    }
}
