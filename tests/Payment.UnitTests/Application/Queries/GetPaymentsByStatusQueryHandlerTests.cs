using DomainPayment = Payment.Domain.AggregatesModel.PaymentAggregate.Payment;
using Payment.UnitTests.Helpers;

namespace Payment.UnitTests.Application.Queries;

public sealed class GetPaymentsByStatusQueryHandlerTests
{
    private readonly IPaymentRepository _paymentRepository = Substitute.For<IPaymentRepository>();
    private readonly GetPaymentsByStatusQueryHandler _handler;

    public GetPaymentsByStatusQueryHandlerTests()
    {
        _handler = new GetPaymentsByStatusQueryHandler(_paymentRepository);
    }

    [Fact]
    public async Task Handle_PaymentsExist_ShouldReturnFilteredList()
    {
        // Arrange
        var status = PaymentStatus.Succeeded;
        var payments = new List<DomainPayment>
        {
            PaymentTestFactory.CreateSucceededPayment(orderId: "ORDER-001", userId: "user-1"),
            PaymentTestFactory.CreateSucceededPayment(orderId: "ORDER-002", userId: "user-2"),
            PaymentTestFactory.CreateSucceededPayment(orderId: "ORDER-003", userId: "user-3")
        };

        _paymentRepository
            .GetByStatusAsync(status, Arg.Any<CancellationToken>())
            .Returns(payments);

        var query = new GetPaymentsByStatusQuery(status);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().AllSatisfy(dto => dto.Status.Should().Be(status));
    }

    [Fact]
    public async Task Handle_NoPayments_ShouldReturnEmptyList()
    {
        // Arrange
        var status = PaymentStatus.Failed;
        _paymentRepository
            .GetByStatusAsync(status, Arg.Any<CancellationToken>())
            .Returns(new List<DomainPayment>());

        var query = new GetPaymentsByStatusQuery(status);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_NullQuery_ShouldThrowArgumentNullException()
    {
        // Act
        var act = async () => await _handler.Handle(null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
