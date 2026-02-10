using Payment.UnitTests.Helpers;
using DomainPayment = Payment.Domain.AggregatesModel.PaymentAggregate.Payment;

namespace Payment.UnitTests.Infrastructure.Repositories;

/// <summary>
/// Testes unitários para o PaymentRepository.
/// Valida operações CRUD e queries específicas do repositório de pagamentos.
/// </summary>
public sealed class PaymentRepositoryTests
{
    [Fact]
    public void UnitOfWork_ShouldReturnContext()
    {
        // Arrange
        using var context = PaymentContextFactory.Create();
        var repository = new PaymentRepository(context);

        // Act
        var unitOfWork = repository.UnitOfWork;

        // Assert
        unitOfWork.Should().NotBeNull();
        unitOfWork.Should().BeSameAs(context);
    }

    [Fact]
    public void Add_ShouldAddPaymentToContext()
    {
        // Arrange
        using var context = PaymentContextFactory.Create();
        var repository = new PaymentRepository(context);
        var payment = PaymentTestFactory.CreateValidPayment(
            orderId: "ORDER-ADD-001",
            userId: "USER-ADD-001"
        );

        // Act
        var addedPayment = repository.Add(payment);

        // Assert
        addedPayment.Should().NotBeNull();
        addedPayment.Should().BeSameAs(payment);
        context.Payments.Local.Should().Contain(payment);
    }

    [Fact]
    public void Update_ShouldMarkPaymentAsModified()
    {
        // Arrange
        using var context = PaymentContextFactory.Create();
        var repository = new PaymentRepository(context);
        var payment = PaymentTestFactory.CreateValidPayment(
            orderId: "ORDER-UPDATE-001",
            userId: "USER-UPDATE-001"
        );

        context.Payments.Add(payment);
        context.SaveChanges();

        // Detach para simular cenário onde payment vem de outra instância
        context.Entry(payment).State = EntityState.Detached;

        // Act
        repository.Update(payment);

        // Assert
        context.Entry(payment).State.Should().Be(EntityState.Modified);
    }

    [Fact]
    public async Task GetByIdAsync_PaymentExists_ShouldReturnPayment()
    {
        // Arrange
        using var context = PaymentContextFactory.Create("test-db-getbyid");
        var repository = new PaymentRepository(context);

        var payment = PaymentTestFactory.CreateValidPayment(
            orderId: "ORDER-GETID-001",
            userId: "USER-GETID-001"
        );

        context.Payments.Add(payment);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var paymentId = payment.Id;

        // Act
        var retrievedPayment = await repository.GetByIdAsync(paymentId, TestContext.Current.CancellationToken);

        // Assert
        retrievedPayment.Should().NotBeNull();
        retrievedPayment!.Id.Should().Be(paymentId);
        retrievedPayment.OrderId.Should().Be("ORDER-GETID-001");
        retrievedPayment.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_PaymentNotExists_ShouldReturnNull()
    {
        // Arrange
        using var context = PaymentContextFactory.Create();
        var repository = new PaymentRepository(context);
        var nonExistentId = PaymentId.NewId();

        // Act
        var retrievedPayment = await repository.GetByIdAsync(nonExistentId, TestContext.Current.CancellationToken);

        // Assert
        retrievedPayment.Should().BeNull();
    }

    [Fact]
    public async Task GetByOrderIdAsync_ShouldReturnPayment()
    {
        // Arrange
        using var context = PaymentContextFactory.Create("test-db-getbyorder");
        var repository = new PaymentRepository(context);

        var payment = PaymentTestFactory.CreateValidPayment(
            orderId: "ORDER-UNIQUE-12345",
            userId: "USER-ORDER-001"
        );

        context.Payments.Add(payment);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var retrievedPayment = await repository.GetByOrderIdAsync("ORDER-UNIQUE-12345", TestContext.Current.CancellationToken);

        // Assert
        retrievedPayment.Should().NotBeNull();
        retrievedPayment!.OrderId.Should().Be("ORDER-UNIQUE-12345");
        retrievedPayment.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetByOrderIdAsync_NotFound_ShouldReturnNull()
    {
        // Arrange
        using var context = PaymentContextFactory.Create();
        var repository = new PaymentRepository(context);

        // Act
        var retrievedPayment = await repository.GetByOrderIdAsync("NON-EXISTENT-ORDER", TestContext.Current.CancellationToken);

        // Assert
        retrievedPayment.Should().BeNull();
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnPayments()
    {
        // Arrange
        using var context = PaymentContextFactory.Create("test-db-getbyuser");
        var repository = new PaymentRepository(context);

        var payment1 = PaymentTestFactory.CreateValidPayment("ORDER-USER-001", "USER-123");
        var payment2 = PaymentTestFactory.CreateValidPayment("ORDER-USER-002", "USER-123");
        var payment3 = PaymentTestFactory.CreateValidPayment("ORDER-USER-003", "USER-456");

        context.Payments.AddRange(payment1, payment2, payment3);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var userPayments = await repository.GetByUserIdAsync("USER-123", TestContext.Current.CancellationToken);

        // Assert
        userPayments.Should().NotBeNull();
        userPayments.Should().HaveCount(2);
        userPayments.Should().AllSatisfy(p => p.UserId.Should().Be("USER-123"));
        userPayments.Should().AllSatisfy(p => p.Items.Should().NotBeEmpty());
    }

    [Fact]
    public async Task GetByUserIdAsync_NoPayments_ShouldReturnEmpty()
    {
        // Arrange
        using var context = PaymentContextFactory.Create();
        var repository = new PaymentRepository(context);

        // Act
        var userPayments = await repository.GetByUserIdAsync("NON-EXISTENT-USER", TestContext.Current.CancellationToken);

        // Assert
        userPayments.Should().NotBeNull();
        userPayments.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByStatusAsync_ShouldReturnFilteredPayments()
    {
        // Arrange
        using var context = PaymentContextFactory.Create("test-db-getbystatus");
        var repository = new PaymentRepository(context);

        var pendingPayment1 = PaymentTestFactory.CreateValidPayment("ORDER-STATUS-001", "USER-001");
        var pendingPayment2 = PaymentTestFactory.CreateValidPayment("ORDER-STATUS-002", "USER-002");
        var succeededPayment = PaymentTestFactory.CreateSucceededPayment("ORDER-STATUS-003", "USER-003");

        context.Payments.AddRange(pendingPayment1, pendingPayment2, succeededPayment);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var pendingPayments = await repository.GetByStatusAsync(PaymentStatus.Pending, TestContext.Current.CancellationToken);

        // Assert
        pendingPayments.Should().NotBeNull();
        pendingPayments.Should().HaveCount(2);
        pendingPayments.Should().AllSatisfy(p => p.Status.Should().Be(PaymentStatus.Pending));
        pendingPayments.Should().AllSatisfy(p => p.Items.Should().NotBeEmpty());
    }

    [Fact]
    public async Task GetByStatusAsync_NoMatchingStatus_ShouldReturnEmpty()
    {
        // Arrange
        using var context = PaymentContextFactory.Create("test-db-nostatus");
        var repository = new PaymentRepository(context);

        var pendingPayment = PaymentTestFactory.CreateValidPayment("ORDER-001", "USER-001");
        context.Payments.Add(pendingPayment);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var refundedPayments = await repository.GetByStatusAsync(PaymentStatus.Refunded, TestContext.Current.CancellationToken);

        // Assert
        refundedPayments.Should().NotBeNull();
        refundedPayments.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllPayments()
    {
        // Arrange
        using var context = PaymentContextFactory.Create("test-db-getall");
        var repository = new PaymentRepository(context);

        var payment1 = PaymentTestFactory.CreateValidPayment("ORDER-ALL-001", "USER-001");
        var payment2 = PaymentTestFactory.CreateValidPayment("ORDER-ALL-002", "USER-002");
        var payment3 = PaymentTestFactory.CreateSucceededPayment("ORDER-ALL-003", "USER-003");

        context.Payments.AddRange(payment1, payment2, payment3);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var allPayments = await repository.GetAllAsync(TestContext.Current.CancellationToken);

        // Assert
        allPayments.Should().NotBeNull();
        allPayments.Should().HaveCount(3);
        allPayments.Should().AllSatisfy(p => p.Items.Should().NotBeEmpty());
    }

    [Fact]
    public async Task GetAllAsync_Empty_ShouldReturnEmptyList()
    {
        // Arrange
        using var context = PaymentContextFactory.Create();
        var repository = new PaymentRepository(context);

        // Act
        var allPayments = await repository.GetAllAsync(TestContext.Current.CancellationToken);

        // Assert
        allPayments.Should().NotBeNull();
        allPayments.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPendingPaymentsOlderThanAsync_ShouldReturnOldPendingPayments()
    {
        // Arrange
        using var context = PaymentContextFactory.Create("test-db-oldpending");
        var repository = new PaymentRepository(context);

        // Cria payment antigo simulando via reflexão (já que CreatedAt é private set)
        var oldPayment = PaymentTestFactory.CreateValidPayment("ORDER-OLD-001", "USER-OLD-001");
        var oldPaymentType = oldPayment.GetType();
        var createdAtProperty = oldPaymentType.GetProperty("CreatedAt");
        createdAtProperty?.SetValue(oldPayment, DateTime.UtcNow.AddMinutes(-35));

        var recentPayment = PaymentTestFactory.CreateValidPayment("ORDER-RECENT-001", "USER-RECENT-001");

        context.Payments.AddRange(oldPayment, recentPayment);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var oldPendingPayments = await repository.GetPendingPaymentsOlderThanAsync(30, TestContext.Current.CancellationToken);

        // Assert
        oldPendingPayments.Should().NotBeNull();
        oldPendingPayments.Should().ContainSingle();
        oldPendingPayments.First().OrderId.Should().Be("ORDER-OLD-001");
    }

    [Fact]
    public async Task GetPendingPaymentsOlderThanAsync_NoPendingOld_ShouldReturnEmpty()
    {
        // Arrange
        using var context = PaymentContextFactory.Create();
        var repository = new PaymentRepository(context);

        var recentPayment = PaymentTestFactory.CreateValidPayment("ORDER-RECENT", "USER-RECENT");
        context.Payments.Add(recentPayment);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var oldPendingPayments = await repository.GetPendingPaymentsOlderThanAsync(30, TestContext.Current.CancellationToken);

        // Assert
        oldPendingPayments.Should().NotBeNull();
        oldPendingPayments.Should().BeEmpty();
    }

    [Fact]
    public async Task Repository_ShouldWorkWithComplexPaymentScenarios()
    {
        // Arrange
        using var context = PaymentContextFactory.Create("test-db-complex");
        var repository = new PaymentRepository(context);

        // Cria diferentes tipos de pagamento
        var pendingPayment = PaymentTestFactory.CreateValidPayment("ORDER-COMPLEX-001", "USER-COMPLEX");
        var processingPayment = PaymentTestFactory.CreateProcessingPayment("ORDER-COMPLEX-002", "USER-COMPLEX");
        var succeededPayment = PaymentTestFactory.CreateSucceededPayment("ORDER-COMPLEX-003", "USER-COMPLEX");
        var failedPayment = PaymentTestFactory.CreateFailedPayment();
        var cancelledPayment = PaymentTestFactory.CreateCancelledPayment();
        var refundedPayment = PaymentTestFactory.CreateRefundedPayment();

        // Act
        context.Payments.AddRange(
            pendingPayment,
            processingPayment,
            succeededPayment,
            failedPayment,
            cancelledPayment,
            refundedPayment
        );
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert - Verifica queries específicas
        var userPayments = await repository.GetByUserIdAsync("USER-COMPLEX", TestContext.Current.CancellationToken);
        userPayments.Should().HaveCount(3);

        var succeededPayments = await repository.GetByStatusAsync(PaymentStatus.Succeeded, TestContext.Current.CancellationToken);
        succeededPayments.Should().HaveCountGreaterThanOrEqualTo(1);

        var allPayments = await repository.GetAllAsync(TestContext.Current.CancellationToken);
        allPayments.Should().HaveCount(6);
    }
}
