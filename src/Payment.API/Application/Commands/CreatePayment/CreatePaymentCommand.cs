namespace Payment.API.Application.Commands.CreatePayment;

public record CreatePaymentCommand(
    string OrderId,
    string UserId,
    PaymentMethod Method,
    IReadOnlyCollection<PaymentItemDto> Items,
    CardInfoDto? CardInfo
) : ICommand<PaymentDto>
{
    public static CreatePaymentCommand FromRequest(CreatePaymentRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return new CreatePaymentCommand(
            request.OrderId,
            request.UserId,
            request.Method,
            request.Items,
            request.CardInfo
        );
    }
}

public class CreatePaymentCommandHandler(IPaymentRepository paymentRepository)
    : ICommandHandler<CreatePaymentCommand, PaymentDto>
{
    public async Task<PaymentDto> Handle(CreatePaymentCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var items = command.Items.Select(i => new Domain.AggregatesModel.PaymentAggregate.PaymentItem(
            i.ProductId,
            i.ProductName,
            i.Quantity,
            Money.FromDecimal(i.UnitPriceAmount, i.UnitPriceCurrency)
        )).ToList();

        CardInfo? cardInfo = null;
        if (command.CardInfo != null)
        {
            cardInfo = CardInfo.Create(
                command.CardInfo.CardHolderName,
                command.CardInfo.CardNumber,
                command.CardInfo.CardBrand,
                command.CardInfo.ExpirationMonth,
                command.CardInfo.ExpirationYear
            );
        }

        var payment = Domain.AggregatesModel.PaymentAggregate.Payment.Create(
            command.OrderId,
            command.UserId,
            command.Method,
            items,
            cardInfo
        );

        paymentRepository.Add(payment);
        await paymentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return PaymentDto.FromPayment(payment);
    }
}
