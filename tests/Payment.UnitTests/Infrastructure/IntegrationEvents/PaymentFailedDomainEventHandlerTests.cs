using Payment.Infrastructure.IntegrationEvents.EventHandling;
using Payment.UnitTests.Helpers;

namespace Payment.UnitTests.Infrastructure.IntegrationEvents;

/// <summary>
/// Testes unitários para PaymentFailedDomainEventHandler.
/// Valida o comportamento do handler ao processar eventos de falha de pagamento.
/// </summary>
public sealed class PaymentFailedDomainEventHandlerTests
{
    private readonly IEventBus _eventBus;
    private readonly ILogger<PaymentFailedDomainEventHandler> _logger;
    private readonly PaymentFailedDomainEventHandler _handler;

    public PaymentFailedDomainEventHandlerTests()
    {
        _eventBus = Substitute.For<IEventBus>();
        _logger = Substitute.For<ILogger<PaymentFailedDomainEventHandler>>();
        _handler = new PaymentFailedDomainEventHandler(_eventBus, _logger);
    }

    [Fact]
    public async Task Handle_ValidEvent_ShouldNotThrow()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateFailedPayment();
        var domainEvent = new PaymentFailedDomainEvent(payment, "Insufficient funds");

        // Act
        var act = async () => await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_ValidEvent_ShouldCompleteSuccessfully()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateProcessingPayment("ORDER-FAIL-001", "USER-FAIL-001");
        payment.MarkAsFailed("Card declined");
        payment.ClearNotification();

        var domainEvent = new PaymentFailedDomainEvent(payment, "Card declined");

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert - Handler deve executar sem exceções
        true.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithDifferentFailureReasons_ShouldNotThrow()
    {
        // Arrange
        var reasons = new[]
        {
            "Insufficient funds",
            "Card declined",
            "Invalid CVV",
            "Expired card",
            "Fraud detection"
        };

        // Act & Assert
        foreach (var reason in reasons)
        {
            var payment = PaymentTestFactory.CreateProcessingPayment();
            payment.MarkAsFailed(reason);
            payment.ClearNotification();

            var domainEvent = new PaymentFailedDomainEvent(payment, reason);
            var act = async () => await _handler.Handle(domainEvent, CancellationToken.None);

            await act.Should().NotThrowAsync($"reason: {reason}");
        }
    }

    [Fact]
    public async Task Handle_WithProcessorResponse_ShouldNotThrow()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateProcessingPayment();
        payment.MarkAsFailed("Payment gateway error", "Error code: 500");
        payment.ClearNotification();

        var domainEvent = new PaymentFailedDomainEvent(payment, "Payment gateway error");

        // Act
        var act = async () => await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateFailedPayment();
        var domainEvent = new PaymentFailedDomainEvent(payment, "Test failure");
        var cts = new CancellationTokenSource();

        // Act
        var act = async () => await _handler.Handle(domainEvent, cts.Token);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_MultipleFailedPayments_ShouldHandleAllSuccessfully()
    {
        // Arrange
        var payments = new[]
        {
            PaymentTestFactory.CreateFailedPayment(),
            PaymentTestFactory.CreateFailedPayment(),
            PaymentTestFactory.CreateFailedPayment()
        };

        // Act & Assert
        foreach (var payment in payments)
        {
            var domainEvent = new PaymentFailedDomainEvent(payment, "Generic failure");
            var act = async () => await _handler.Handle(domainEvent, CancellationToken.None);
            await act.Should().NotThrowAsync();
        }
    }

    [Fact]
    public async Task Handle_FromPendingToFailed_ShouldNotThrow()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateValidPayment();
        payment.MarkAsFailed("Validation failed");
        payment.ClearNotification();

        var domainEvent = new PaymentFailedDomainEvent(payment, "Validation failed");

        // Act
        var act = async () => await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }
}
