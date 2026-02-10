using DomainPayment = Payment.Domain.AggregatesModel.PaymentAggregate.Payment;
using Payment.UnitTests.Helpers;

namespace Payment.UnitTests.Application.Commands;

public sealed class ProcessPaymentCommandHandlerTests
{
    private readonly IPaymentRepository _paymentRepository = Substitute.For<IPaymentRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ProcessPaymentCommandHandler _handler;

    public ProcessPaymentCommandHandlerTests()
    {
        _paymentRepository.UnitOfWork.Returns(_unitOfWork);
        _unitOfWork.SaveEntitiesAsync(Arg.Any<CancellationToken>()).Returns(true);
        _handler = new ProcessPaymentCommandHandler(_paymentRepository);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldProcessAndReturnDto()
    {
        // Arrange
        var payment = PaymentTestFactory.CreateValidPayment();
        _paymentRepository
            .GetByIdAsync(Arg.Any<PaymentId>(), Arg.Any<CancellationToken>())
            .Returns(payment);

        var command = new ProcessPaymentCommand(
            PaymentId: payment.Id.ToString(),
            TransactionId: "TXN-12345",
            AuthorizationCode: "AUTH-67890"
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(PaymentStatus.Succeeded);
        result.TransactionId.Should().Be("TXN-12345");
        result.AuthorizationCode.Should().Be("AUTH-67890");
        result.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_PaymentNotFound_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        _paymentRepository
            .GetByIdAsync(Arg.Any<PaymentId>(), Arg.Any<CancellationToken>())
            .Returns((DomainPayment?)null);

        var command = new ProcessPaymentCommand(
            PaymentId: PaymentId.NewId().ToString(),
            TransactionId: "TXN-12345",
            AuthorizationCode: "AUTH-67890"
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

        var command = new ProcessPaymentCommand(
            PaymentId: payment.Id.ToString(),
            TransactionId: "TXN-12345",
            AuthorizationCode: "AUTH-67890"
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _paymentRepository.Received(1).Update(Arg.Any<DomainPayment>());
        await _unitOfWork.Received(1).SaveEntitiesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public void FromRequest_ValidRequest_ReturnsCommand()
    {
        // Arrange
        var paymentId = PaymentId.NewId().ToString();
        var request = new ProcessPaymentRequest
        {
            TransactionId = "TXN-12345",
            AuthorizationCode = "AUTH-67890"
        };

        // Act
        var command = ProcessPaymentCommand.FromRequest(paymentId, request);

        // Assert
        command.Should().NotBeNull();
        command.PaymentId.Should().Be(paymentId);
        command.TransactionId.Should().Be(request.TransactionId);
        command.AuthorizationCode.Should().Be(request.AuthorizationCode);
    }

    [Fact]
    public void FromRequest_NullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        var paymentId = PaymentId.NewId().ToString();

        // Act
        var act = () => ProcessPaymentCommand.FromRequest(paymentId, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
