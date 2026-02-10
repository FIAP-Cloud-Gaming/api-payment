using Payment.UnitTests.Helpers;
using DomainPayment = Payment.Domain.AggregatesModel.PaymentAggregate.Payment;

namespace Payment.UnitTests.Infrastructure;

/// <summary>
/// Testes unitários para o PaymentContext da camada de infraestrutura.
/// Valida o comportamento do DbContext incluindo configuração de DbSets,
/// persistência de dados e gerenciamento de notificações de domínio.
/// </summary>
public sealed class PaymentContextTests
{
    [Fact]
    public void Payments_DbSet_ShouldNotBeNull()
    {
        // Arrange
        using var context = PaymentContextFactory.Create();

        // Act & Assert
        context.Payments.Should().NotBeNull();
    }

    [Fact]
    public void PaymentItems_DbSet_ShouldNotBeNull()
    {
        // Arrange
        using var context = PaymentContextFactory.Create();

        // Act & Assert
        context.PaymentItems.Should().NotBeNull();
    }

    [Fact]
    public async Task SaveEntitiesAsync_ShouldReturnTrue()
    {
        // Arrange
        using var context = PaymentContextFactory.Create();

        // Act
        var result = await context.SaveEntitiesAsync(CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SaveEntitiesAsync_ShouldPersistPaymentData()
    {
        // Arrange
        using var context = PaymentContextFactory.Create("test-db-persist");
        var payment = PaymentTestFactory.CreateValidPayment(
            orderId: "ORDER-PERSIST-001",
            userId: "USER-PERSIST-001"
        );

        // Act
        context.Payments.Add(payment);
        await context.SaveEntitiesAsync(CancellationToken.None);

        // Assert - Verifica que o pagamento foi persistido
        var savedPayment = await context.Payments
            .FirstOrDefaultAsync(p => p.OrderId == "ORDER-PERSIST-001");

        savedPayment.Should().NotBeNull();
        savedPayment!.UserId.Should().Be("USER-PERSIST-001");
        savedPayment.Status.Should().Be(PaymentStatus.Pending);
    }

    [Fact]
    public void GetEntitiesWithNotifications_WithNotifications_ShouldReturnEntities()
    {
        // Arrange
        using var context = PaymentContextFactory.Create();

        // Cria um payment que terá notificações ao ser criado
        var payment = DomainPayment.Create(
            orderId: "ORDER-NOTIF-001",
            userId: "USER-NOTIF-001",
            method: PaymentMethod.Pix,
            items: PaymentTestFactory.CreateValidItems(1)
        );

        context.Payments.Add(payment);

        // Act
        var entitiesWithNotifications = context.GetEntitiesWithNotifications().ToList();

        // Assert
        entitiesWithNotifications.Should().NotBeEmpty();
        entitiesWithNotifications.Should().ContainSingle();
        entitiesWithNotifications.First().Should().Be(payment);
        entitiesWithNotifications.First().Notifications.Should().NotBeEmpty();
    }

    [Fact]
    public void GetEntitiesWithNotifications_WithoutNotifications_ShouldReturnEmpty()
    {
        // Arrange
        using var context = PaymentContextFactory.Create();
        var payment = PaymentTestFactory.CreateValidPayment(
            orderId: "ORDER-NO-NOTIF-001",
            userId: "USER-NO-NOTIF-001"
        );

        // O PaymentTestFactory já limpa as notificações via ClearNotification()
        context.Payments.Add(payment);

        // Act
        var entitiesWithNotifications = context.GetEntitiesWithNotifications().ToList();

        // Assert
        entitiesWithNotifications.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveEntitiesAsync_ShouldDispatchDomainEvents()
    {
        // Arrange
        using var context = PaymentContextFactory.Create();

        // Cria payment com notificação de domínio
        var payment = DomainPayment.Create(
            orderId: "ORDER-EVENT-001",
            userId: "USER-EVENT-001",
            method: PaymentMethod.CreditCard,
            items: PaymentTestFactory.CreateValidItems(1),
            cardInfo: CardInfo.Create(
                "John Doe",
                "4111111111111111",
                "Visa",
                12,
                DateTime.UtcNow.Year + 2
            )
        );

        context.Payments.Add(payment);

        // Act
        var result = await context.SaveEntitiesAsync(CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        // Após SaveEntitiesAsync, o mediator deve ter despachado as notificações
        // e as notificações devem ter sido limpas da entidade
        payment.Notifications.Should().BeEmpty();
    }

    [Fact]
    public async Task PaymentContext_ShouldSupportMultiplePayments()
    {
        // Arrange
        using var context = PaymentContextFactory.Create("test-db-multiple");

        var payment1 = PaymentTestFactory.CreateValidPayment("ORDER-001", "USER-001");
        var payment2 = PaymentTestFactory.CreateValidPayment("ORDER-002", "USER-002");
        var payment3 = PaymentTestFactory.CreateValidPayment("ORDER-003", "USER-001");

        // Act
        context.Payments.AddRange(payment1, payment2, payment3);
        await context.SaveEntitiesAsync(CancellationToken.None);

        // Assert
        var allPayments = await context.Payments.ToListAsync();
        allPayments.Should().HaveCount(3);
    }
}
