using DomainPayment = Payment.Domain.AggregatesModel.PaymentAggregate.Payment;
using Payment.UnitTests.Helpers;

namespace Payment.UnitTests.Application.Queries;

public sealed class GetPaymentQueryHandlerTests
{
    private readonly IPaymentRepository _paymentRepository = Substitute.For<IPaymentRepository>();
    private readonly GetPaymentQueryHandler _handler;

    public GetPaymentQueryHandlerTests()
    {
        _handler = new GetPaymentQueryHandler(_paymentRepository);
    }

    [Fact]
    public async Task Handle_PaymentExists_ShouldReturnDto()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateValidPayment();
        _paymentRepository
            .GetByIdAsync(Arg.Any<PaymentId>(), Arg.Any<CancellationToken>())
            .Returns(payment);

        var query = new GetPaymentQuery(payment.Id.ToString());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(payment.Id.ToString());
        result.OrderId.Should().Be(payment.OrderId);
        result.UserId.Should().Be(payment.UserId);
        result.Status.Should().Be(payment.Status);
    }

    [Fact]
    public async Task Handle_PaymentNotFound_ShouldReturnNull()
    {
        // Arrange
        _paymentRepository
            .GetByIdAsync(Arg.Any<PaymentId>(), Arg.Any<CancellationToken>())
            .Returns((DomainPayment?)null);

        var query = new GetPaymentQuery(PaymentId.NewId().ToString());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
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
