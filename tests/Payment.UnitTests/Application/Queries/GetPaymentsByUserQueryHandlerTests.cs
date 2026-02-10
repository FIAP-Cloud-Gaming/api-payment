using DomainPayment = Payment.Domain.AggregatesModel.PaymentAggregate.Payment;
using Payment.UnitTests.Helpers;

namespace Payment.UnitTests.Application.Queries;

public sealed class GetPaymentsByUserQueryHandlerTests
{
    private readonly IPaymentRepository _paymentRepository = Substitute.For<IPaymentRepository>();
    private readonly GetPaymentsByUserQueryHandler _handler;

    public GetPaymentsByUserQueryHandlerTests()
    {
        _handler = new GetPaymentsByUserQueryHandler(_paymentRepository);
    }

    [Fact]
    public async Task Handle_UserHasPayments_ShouldReturnList()
    {
        // Arrange
        var userId = "user-123";
        var payments = new List<DomainPayment>
        {
            PaymentTestFactory.CreateValidPayment(orderId: "ORDER-001", userId: userId),
            PaymentTestFactory.CreateValidPayment(orderId: "ORDER-002", userId: userId),
            PaymentTestFactory.CreateSucceededPayment(orderId: "ORDER-003", userId: userId)
        };

        _paymentRepository
            .GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(payments);

        var query = new GetPaymentsByUserQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().AllSatisfy(dto => dto.UserId.Should().Be(userId));
    }

    [Fact]
    public async Task Handle_UserHasNoPayments_ShouldReturnEmptyList()
    {
        // Arrange
        var userId = "user-no-payments";
        _paymentRepository
            .GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<DomainPayment>());

        var query = new GetPaymentsByUserQuery(userId);

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
