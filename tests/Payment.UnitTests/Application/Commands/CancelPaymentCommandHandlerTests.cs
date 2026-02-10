using DomainPayment = Payment.Domain.AggregatesModel.PaymentAggregate.Payment;
using Payment.UnitTests.Helpers;

namespace Payment.UnitTests.Application.Commands;

public sealed class CancelPaymentCommandHandlerTests
{
    private readonly IPaymentRepository _paymentRepository = Substitute.For<IPaymentRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CancelPaymentCommandHandler _handler;

    public CancelPaymentCommandHandlerTests()
    {
        _paymentRepository.UnitOfWork.Returns(_unitOfWork);
        _unitOfWork.SaveEntitiesAsync(Arg.Any<CancellationToken>()).Returns(true);
        _handler = new CancelPaymentCommandHandler(_paymentRepository);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCancelAndReturnDto()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateValidPayment();
        _paymentRepository
            .GetByIdAsync(Arg.Any<PaymentId>(), Arg.Any<CancellationToken>())
            .Returns(payment);

        var command = new CancelPaymentCommand(
            PaymentId: payment.Id.ToString(),
            Reason: "User requested cancellation"
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(PaymentStatus.Cancelled);
        result.FailureReason.Should().Contain("User requested cancellation");
    }

    [Fact]
    public async Task Handle_PaymentNotFound_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        _paymentRepository
            .GetByIdAsync(Arg.Any<PaymentId>(), Arg.Any<CancellationToken>())
            .Returns((DomainPayment?)null);

        var command = new CancelPaymentCommand(
            PaymentId: PaymentId.NewId().ToString(),
            Reason: "User requested cancellation"
        );

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_NullCommand_ShouldThrowArgumentNullException()
    {
        // Act
        var act = async () => await _handler.Handle(null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Handle_ShouldCallUpdateAndSave()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateValidPayment();
        _paymentRepository
            .GetByIdAsync(Arg.Any<PaymentId>(), Arg.Any<CancellationToken>())
            .Returns(payment);

        var command = new CancelPaymentCommand(
            PaymentId: payment.Id.ToString(),
            Reason: "User requested cancellation"
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _paymentRepository.Received(1).Update(Arg.Any<DomainPayment>());
        await _unitOfWork.Received(1).SaveEntitiesAsync(Arg.Any<CancellationToken>());
    }
}
