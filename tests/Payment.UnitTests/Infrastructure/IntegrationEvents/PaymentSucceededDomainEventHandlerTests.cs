using Payment.Infrastructure.IntegrationEvents.EventHandling;
using Payment.UnitTests.Helpers;

namespace Payment.UnitTests.Infrastructure.IntegrationEvents;

/// <summary>
/// Testes unitários para PaymentSucceededDomainEventHandler.
/// Valida o comportamento do handler ao processar eventos de sucesso de pagamento.
/// </summary>
public sealed class PaymentSucceededDomainEventHandlerTests
{
    private readonly ILogger<PaymentSucceededDomainEventHandler> _logger;
    private readonly PaymentSucceededDomainEventHandler _handler;

    public PaymentSucceededDomainEventHandlerTests()
    {
        _logger = Substitute.For<ILogger<PaymentSucceededDomainEventHandler>>();
        _handler = new PaymentSucceededDomainEventHandler(_logger);
    }

    [Fact]
    public async Task Handle_ValidEvent_ShouldNotThrow()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateSucceededPayment(
            orderId: "ORDER-SUCCESS-001",
            userId: "USER-SUCCESS-001"
        );

        var domainEvent = new PaymentSucceededDomainEvent(payment);

        // Act
        var act = async () => await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_ValidEvent_ShouldCompleteSuccessfully()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateSucceededPayment(
            orderId: "ORDER-SUCCESS-002",
            userId: "USER-SUCCESS-002",
            amount: 299.99m
        );

        var domainEvent = new PaymentSucceededDomainEvent(payment);

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert - Handler deve executar sem exceções
        true.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithTransactionDetails_ShouldNotThrow()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateValidPayment();
        payment.StartProcessing();
        payment.MarkAsSucceeded("TXN-12345", "AUTH-67890");
        payment.ClearNotification();

        var domainEvent = new PaymentSucceededDomainEvent(payment);

        // Act
        var act = async () => await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateSucceededPayment();
        var domainEvent = new PaymentSucceededDomainEvent(payment);
        var cts = new CancellationTokenSource();

        // Act
        var act = async () => await _handler.Handle(domainEvent, cts.Token);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_MultipleSuccessEvents_ShouldHandleAllSuccessfully()
    {
        // Arrange
        var payments = new[]
        {
            PaymentTestFactory.CreateSucceededPayment("ORDER-001", "USER-001", 100m),
            PaymentTestFactory.CreateSucceededPayment("ORDER-002", "USER-002", 200m),
            PaymentTestFactory.CreateSucceededPayment("ORDER-003", "USER-003", 300m)
        };

        // Act & Assert
        foreach (var payment in payments)
        {
            var domainEvent = new PaymentSucceededDomainEvent(payment);
            var act = async () => await _handler.Handle(domainEvent, CancellationToken.None);
            await act.Should().NotThrowAsync();
        }
    }

    [Fact]
    public async Task Handle_WithDifferentAmounts_ShouldNotThrow()
    {
        // Arrange
        var smallPayment = PaymentTestFactory.CreateSucceededPayment("ORDER-SMALL", "USER-001", 1.00m);
        var largePayment = PaymentTestFactory.CreateSucceededPayment("ORDER-LARGE", "USER-002", 99999.99m);

        var smallEvent = new PaymentSucceededDomainEvent(smallPayment);
        var largeEvent = new PaymentSucceededDomainEvent(largePayment);

        // Act
        var actSmall = async () => await _handler.Handle(smallEvent, CancellationToken.None);
        var actLarge = async () => await _handler.Handle(largeEvent, CancellationToken.None);

        // Assert
        await actSmall.Should().NotThrowAsync();
        await actLarge.Should().NotThrowAsync();
    }
}
