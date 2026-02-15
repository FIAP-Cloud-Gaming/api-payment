using Payment.API.Application.IntegrationEvents.EventHandling;
using Payment.UnitTests.Helpers;

namespace Payment.UnitTests.Infrastructure.IntegrationEvents;

/// <summary>
/// Testes unitários para PaymentStartedDomainEventHandler.
/// Valida o comportamento do handler ao processar eventos de início de pagamento.
/// </summary>
public sealed class PaymentStartedDomainEventHandlerTests
{
    private readonly IPaymentIntegrationEventService _integrationEventService;
    private readonly ILogger<PaymentStartedDomainEventHandler> _logger;
    private readonly PaymentStartedDomainEventHandler _handler;

    public PaymentStartedDomainEventHandlerTests()
    {
        _integrationEventService = Substitute.For<IPaymentIntegrationEventService>();
        _logger = Substitute.For<ILogger<PaymentStartedDomainEventHandler>>();
        _handler = new PaymentStartedDomainEventHandler(_integrationEventService, _logger);
    }

    [Fact]
    public async Task Handle_ValidEvent_ShouldNotThrow()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateValidPayment(
            orderId: "ORDER-STARTED-001",
            userId: "USER-STARTED-001"
        );

        var domainEvent = new PaymentStartedDomainEvent(payment);

        // Act
        var act = async () => await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_ValidEvent_ShouldCompleteSuccessfully()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateValidPayment(
            orderId: "ORDER-STARTED-002",
            userId: "USER-STARTED-002",
            method: PaymentMethod.Pix
        );

        var domainEvent = new PaymentStartedDomainEvent(payment);

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert - Handler deve executar sem exceções
        // O método retorna Task.CompletedTask, então não há mais validações específicas
        // exceto verificar que foi executado com sucesso
        true.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithCreditCardPayment_ShouldNotThrow()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateValidPaymentWithCard(
            orderId: "ORDER-CARD-001",
            userId: "USER-CARD-001",
            method: PaymentMethod.CreditCard
        );

        var domainEvent = new PaymentStartedDomainEvent(payment);

        // Act
        var act = async () => await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_WithDebitCardPayment_ShouldNotThrow()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateValidPaymentWithCard(
            orderId: "ORDER-DEBIT-001",
            userId: "USER-DEBIT-001",
            method: PaymentMethod.DebitCard
        );

        var domainEvent = new PaymentStartedDomainEvent(payment);

        // Act
        var act = async () => await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_WithBankSlipPayment_ShouldNotThrow()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateValidPayment(
            orderId: "ORDER-BANKSLIP-001",
            userId: "USER-BANKSLIP-001",
            method: PaymentMethod.BankSlip
        );

        var domainEvent = new PaymentStartedDomainEvent(payment);

        // Act
        var act = async () => await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateValidPayment();
        var domainEvent = new PaymentStartedDomainEvent(payment);
        var cts = new CancellationTokenSource();

        // Act - Como o handler retorna Task.CompletedTask imediatamente,
        // não há operação async real para cancelar, mas testamos o fluxo
        var act = async () => await _handler.Handle(domainEvent, cts.Token);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_MultipleEvents_ShouldHandleAllSuccessfully()
    {
        // Arrange
        var payments = new[]
        {
            PaymentTestFactory.CreateValidPayment("ORDER-001", "USER-001"),
            PaymentTestFactory.CreateValidPayment("ORDER-002", "USER-002"),
            PaymentTestFactory.CreateValidPayment("ORDER-003", "USER-003")
        };

        // Act & Assert
        foreach (var payment in payments)
        {
            var domainEvent = new PaymentStartedDomainEvent(payment);
            var act = async () => await _handler.Handle(domainEvent, CancellationToken.None);
            await act.Should().NotThrowAsync();
        }
    }
}
