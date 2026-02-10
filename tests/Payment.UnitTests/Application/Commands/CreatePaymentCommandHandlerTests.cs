using DomainPayment = Payment.Domain.AggregatesModel.PaymentAggregate.Payment;
using Payment.UnitTests.Helpers;

namespace Payment.UnitTests.Application.Commands;

public sealed class CreatePaymentCommandHandlerTests
{
    private readonly IPaymentRepository _paymentRepository = Substitute.For<IPaymentRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreatePaymentCommandHandler _handler;

    public CreatePaymentCommandHandlerTests()
    {
        _paymentRepository.UnitOfWork.Returns(_unitOfWork);
        _unitOfWork.SaveEntitiesAsync(Arg.Any<CancellationToken>()).Returns(true);
        _handler = new CreatePaymentCommandHandler(_paymentRepository);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldReturnPaymentDto()
    {
        // Arrange
        var command = new CreatePaymentCommand(
            OrderId: "ORDER-001",
            UserId: "user-123",
            Method: PaymentMethod.Pix,
            Items: new List<PaymentItemDto>
            {
                new() { ProductId = "prod-1", ProductName = "Product 1", Quantity = 1, UnitPriceAmount = 99.99m, UnitPriceCurrency = "BRL" }
            },
            CardInfo: null
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.OrderId.Should().Be("ORDER-001");
        result.UserId.Should().Be("user-123");
        result.Method.Should().Be(PaymentMethod.Pix);
        result.Status.Should().Be(PaymentStatus.Pending);
        result.Items.Should().HaveCount(1);
        result.TotalAmount.Should().Be(99.99m);
        result.Currency.Should().Be("BRL");
    }

    [Fact]
    public async Task Handle_ShouldCallAddAndSave()
    {
        // Arrange
        var command = new CreatePaymentCommand(
            OrderId: "ORDER-001",
            UserId: "user-123",
            Method: PaymentMethod.Pix,
            Items: new List<PaymentItemDto>
            {
                new() { ProductId = "prod-1", ProductName = "Product 1", Quantity = 1, UnitPriceAmount = 99.99m, UnitPriceCurrency = "BRL" }
            },
            CardInfo: null
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _paymentRepository.Received(1).Add(Arg.Any<DomainPayment>());
        await _unitOfWork.Received(1).SaveEntitiesAsync(Arg.Any<CancellationToken>());
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
    public async Task Handle_WithCardPayment_ShouldIncludeCardInfo()
    {
        // Arrange
        var cardInfo = new CardInfoDto
        {
            CardHolderName = "John Doe",
            CardNumber = "4111111111111111",
            CardBrand = "Visa",
            ExpirationMonth = 12,
            ExpirationYear = DateTime.UtcNow.Year + 2
        };

        var command = new CreatePaymentCommand(
            OrderId: "ORDER-001",
            UserId: "user-123",
            Method: PaymentMethod.CreditCard,
            Items: new List<PaymentItemDto>
            {
                new() { ProductId = "prod-1", ProductName = "Product 1", Quantity = 1, UnitPriceAmount = 99.99m, UnitPriceCurrency = "BRL" }
            },
            CardInfo: cardInfo
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.CardInfo.Should().NotBeNull();
        result.CardInfo!.CardHolderName.Should().Be("John Doe");
        result.CardInfo.CardBrand.Should().Be("Visa");
        result.CardInfo.ExpirationMonth.Should().Be(12);
    }

    [Fact]
    public async Task Handle_WithMultipleItems_ShouldCalculateTotal()
    {
        // Arrange
        var command = new CreatePaymentCommand(
            OrderId: "ORDER-001",
            UserId: "user-123",
            Method: PaymentMethod.Pix,
            Items: new List<PaymentItemDto>
            {
                new() { ProductId = "prod-1", ProductName = "Product 1", Quantity = 2, UnitPriceAmount = 50.00m, UnitPriceCurrency = "BRL" },
                new() { ProductId = "prod-2", ProductName = "Product 2", Quantity = 1, UnitPriceAmount = 150.00m, UnitPriceCurrency = "BRL" }
            },
            CardInfo: null
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalAmount.Should().Be(250.00m); // (2 * 50) + (1 * 150)
    }

    [Fact]
    public void FromRequest_ValidRequest_ReturnsCommand()
    {
        // Arrange
        var request = new CreatePaymentRequest
        {
            OrderId = "ORDER-001",
            UserId = "user-123",
            Method = PaymentMethod.Pix,
            Items = new List<PaymentItemDto>
            {
                new() { ProductId = "prod-1", ProductName = "Product 1", Quantity = 1, UnitPriceAmount = 99.99m, UnitPriceCurrency = "BRL" }
            },
            CardInfo = null
        };

        // Act
        var command = CreatePaymentCommand.FromRequest(request);

        // Assert
        command.Should().NotBeNull();
        command.OrderId.Should().Be(request.OrderId);
        command.UserId.Should().Be(request.UserId);
        command.Method.Should().Be(request.Method);
        command.Items.Should().BeEquivalentTo(request.Items);
    }

    [Fact]
    public void FromRequest_NullRequest_ThrowsArgumentNullException()
    {
        // Act
        var act = () => CreatePaymentCommand.FromRequest(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
