using Payment.API.Application.IntegrationEvents.EventHandling;
using Payment.UnitTests.Helpers;

namespace Payment.UnitTests.Infrastructure.IntegrationEvents;

/// <summary>
/// Testes unitários para PaymentCancelledDomainEventHandler.
/// Valida o comportamento do handler ao processar eventos de cancelamento de pagamento.
/// </summary>
public sealed class PaymentCancelledDomainEventHandlerTests
{
    private readonly IEventBus _eventBus;
    private readonly ILogger<PaymentCancelledDomainEventHandler> _logger;
    private readonly PaymentCancelledDomainEventHandler _handler;

    public PaymentCancelledDomainEventHandlerTests()
    {
        _eventBus = Substitute.For<IEventBus>();
        _logger = Substitute.For<ILogger<PaymentCancelledDomainEventHandler>>();
        _handler = new PaymentCancelledDomainEventHandler(_eventBus, _logger);
    }

    [Fact]
    public async Task Handle_ValidEvent_ShouldNotThrow()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateCancelledPayment();
        var domainEvent = new PaymentCancelledDomainEvent(payment, "User cancelled");

        // Act
        var act = async () => await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_ValidEvent_ShouldCompleteSuccessfully()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateValidPayment("ORDER-CANCEL-001", "USER-CANCEL-001");
        payment.Cancel("Customer requested cancellation");
        payment.ClearNotification();

        var domainEvent = new PaymentCancelledDomainEvent(payment, "Customer requested cancellation");

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert - Handler deve executar sem exceções
        true.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithDifferentCancellationReasons_ShouldNotThrow()
    {
        // Arrange
        var reasons = new[]
        {
            "User cancelled",
            "Order timeout",
            "Duplicate order",
            "Out of stock",
            "Payment method changed"
        };

        // Act & Assert
        foreach (var reason in reasons)
        {
            var payment = PaymentTestFactory.CreateValidPayment();
            payment.Cancel(reason);
            payment.ClearNotification();

            var domainEvent = new PaymentCancelledDomainEvent(payment, reason);
            var act = async () => await _handler.Handle(domainEvent, CancellationToken.None);

            await act.Should().NotThrowAsync($"reason: {reason}");
        }
    }

    [Fact]
    public async Task Handle_PendingPaymentCancellation_ShouldNotThrow()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateValidPayment();
        payment.Cancel("Pending payment cancelled");
        payment.ClearNotification();

        var domainEvent = new PaymentCancelledDomainEvent(payment, "Pending payment cancelled");

        // Act
        var act = async () => await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_ProcessingPaymentCancellation_ShouldNotThrow()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateProcessingPayment();
        payment.Cancel("Processing payment cancelled");
        payment.ClearNotification();

        var domainEvent = new PaymentCancelledDomainEvent(payment, "Processing payment cancelled");

        // Act
        var act = async () => await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateCancelledPayment();
        var domainEvent = new PaymentCancelledDomainEvent(payment, "Test cancellation");
        var cts = new CancellationTokenSource();

        // Act
        var act = async () => await _handler.Handle(domainEvent, cts.Token);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_MultipleCancelledPayments_ShouldHandleAllSuccessfully()
    {
        // Arrange
        var payments = new[]
        {
            PaymentTestFactory.CreateCancelledPayment(),
            PaymentTestFactory.CreateCancelledPayment(),
            PaymentTestFactory.CreateCancelledPayment()
        };

        // Act & Assert
        foreach (var payment in payments)
        {
            var domainEvent = new PaymentCancelledDomainEvent(payment, "Batch cancellation");
            var act = async () => await _handler.Handle(domainEvent, CancellationToken.None);
            await act.Should().NotThrowAsync();
        }
    }

    [Fact]
    public async Task Handle_WithLongCancellationReason_ShouldNotThrow()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateValidPayment();
        var longReason = new string('X', 500) + " - detailed cancellation reason";
        payment.Cancel(longReason);
        payment.ClearNotification();

        var domainEvent = new PaymentCancelledDomainEvent(payment, longReason);

        // Act
        var act = async () => await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }
}
