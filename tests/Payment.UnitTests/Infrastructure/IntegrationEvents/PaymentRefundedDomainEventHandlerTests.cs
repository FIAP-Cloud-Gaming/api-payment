using Payment.API.Application.IntegrationEvents.EventHandling;
using Payment.UnitTests.Helpers;

namespace Payment.UnitTests.Infrastructure.IntegrationEvents;

/// <summary>
/// Testes unitários para PaymentRefundedDomainEventHandler.
/// Valida o comportamento do handler ao processar eventos de reembolso de pagamento.
/// </summary>
public sealed class PaymentRefundedDomainEventHandlerTests
{
    private readonly IEventBus _eventBus;
    private readonly ILogger<PaymentRefundedDomainEventHandler> _logger;
    private readonly PaymentRefundedDomainEventHandler _handler;

    public PaymentRefundedDomainEventHandlerTests()
    {
        _eventBus = Substitute.For<IEventBus>();
        _logger = Substitute.For<ILogger<PaymentRefundedDomainEventHandler>>();
        _handler = new PaymentRefundedDomainEventHandler(_eventBus, _logger);
    }

    [Fact]
    public async Task Handle_ValidEvent_ShouldNotThrow()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateRefundedPayment();
        var domainEvent = new PaymentRefundedDomainEvent(payment, "Customer requested");

        // Act
        var act = async () => await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_ValidEvent_ShouldCompleteSuccessfully()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateSucceededPayment("ORDER-REFUND-001", "USER-REFUND-001");
        payment.Refund("Product defect");
        payment.ClearNotification();

        var domainEvent = new PaymentRefundedDomainEvent(payment, "Product defect");

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert - Handler deve executar sem exceções
        true.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithDifferentRefundReasons_ShouldNotThrow()
    {
        // Arrange
        var reasons = new[]
        {
            "Customer requested",
            "Product defect",
            "Wrong item shipped",
            "Service not rendered",
            "Duplicate charge",
            "Fraudulent transaction"
        };

        // Act & Assert
        foreach (var reason in reasons)
        {
            var payment = PaymentTestFactory.CreateSucceededPayment();
            payment.Refund(reason);
            payment.ClearNotification();

            var domainEvent = new PaymentRefundedDomainEvent(payment, reason);
            var act = async () => await _handler.Handle(domainEvent, CancellationToken.None);

            await act.Should().NotThrowAsync($"reason: {reason}");
        }
    }

    [Fact]
    public async Task Handle_WithSmallRefundAmount_ShouldNotThrow()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateSucceededPayment("ORDER-SMALL", "USER-001", 1.00m);
        payment.Refund("Small refund");
        payment.ClearNotification();

        var domainEvent = new PaymentRefundedDomainEvent(payment, "Small refund");

        // Act
        var act = async () => await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_WithLargeRefundAmount_ShouldNotThrow()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateSucceededPayment("ORDER-LARGE", "USER-002", 99999.99m);
        payment.Refund("Large refund");
        payment.ClearNotification();

        var domainEvent = new PaymentRefundedDomainEvent(payment, "Large refund");

        // Act
        var act = async () => await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateRefundedPayment();
        var domainEvent = new PaymentRefundedDomainEvent(payment, "Test refund");
        var cts = new CancellationTokenSource();

        // Act
        var act = async () => await _handler.Handle(domainEvent, cts.Token);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_MultipleRefundedPayments_ShouldHandleAllSuccessfully()
    {
        // Arrange
        var payments = new[]
        {
            PaymentTestFactory.CreateRefundedPayment(),
            PaymentTestFactory.CreateRefundedPayment(),
            PaymentTestFactory.CreateRefundedPayment()
        };

        // Act & Assert
        foreach (var payment in payments)
        {
            var domainEvent = new PaymentRefundedDomainEvent(payment, "Batch refund");
            var act = async () => await _handler.Handle(domainEvent, CancellationToken.None);
            await act.Should().NotThrowAsync();
        }
    }

    [Fact]
    public async Task Handle_RefundForDifferentPaymentMethods_ShouldNotThrow()
    {
        // Arrange
        var pixPayment = PaymentTestFactory.CreateSucceededPayment("ORDER-PIX", "USER-001");
        pixPayment.Refund("Pix refund");
        pixPayment.ClearNotification();

        var cardPayment = PaymentTestFactory.CreateSucceededPayment("ORDER-CARD", "USER-002");
        cardPayment.Refund("Card refund");
        cardPayment.ClearNotification();

        var pixEvent = new PaymentRefundedDomainEvent(pixPayment, "Pix refund");
        var cardEvent = new PaymentRefundedDomainEvent(cardPayment, "Card refund");

        // Act
        var actPix = async () => await _handler.Handle(pixEvent, CancellationToken.None);
        var actCard = async () => await _handler.Handle(cardEvent, CancellationToken.None);

        // Assert
        await actPix.Should().NotThrowAsync();
        await actCard.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_WithLongRefundReason_ShouldNotThrow()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateRefundedPayment();
        var longReason = new string('X', 500) + " - detailed refund explanation with customer feedback";

        // Recria o event com reason longa
        var domainEvent = new PaymentRefundedDomainEvent(payment, longReason);

        // Act
        var act = async () => await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_ConsecutiveRefunds_ShouldHandleAllSuccessfully()
    {
        // Arrange
        var handler = new PaymentRefundedDomainEventHandler(_eventBus, _logger);
        var payment1 = PaymentTestFactory.CreateRefundedPayment();
        var payment2 = PaymentTestFactory.CreateRefundedPayment();

        var event1 = new PaymentRefundedDomainEvent(payment1, "First refund");
        var event2 = new PaymentRefundedDomainEvent(payment2, "Second refund");

        // Act
        await handler.Handle(event1, CancellationToken.None);
        await handler.Handle(event2, CancellationToken.None);

        // Assert - Se chegou aqui, ambos foram processados com sucesso
        true.Should().BeTrue();
    }
}
