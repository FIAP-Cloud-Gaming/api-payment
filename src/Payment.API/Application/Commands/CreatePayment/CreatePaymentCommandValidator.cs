namespace Payment.API.Application.Commands.CreatePayment;

public class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.Method)
            .IsInEnum().WithMessage("Invalid payment method");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one payment item is required")
            .Must(items => items.All(i => i.Quantity > 0))
                .WithMessage("All items must have quantity greater than zero")
            .Must(items => items.All(i => i.UnitPriceAmount > 0))
                .WithMessage("All items must have unit price greater than zero");

        When(x => x.Method == PaymentMethod.CreditCard || x.Method == PaymentMethod.DebitCard, () =>
        {
            RuleFor(x => x.CardInfo)
                .NotNull().WithMessage("Card information is required for card payments");

            RuleFor(x => x.CardInfo!.CardHolderName)
                .NotEmpty().WithMessage("Card holder name is required");

            RuleFor(x => x.CardInfo!.CardNumber)
                .NotEmpty().WithMessage("Card number is required")
                .MinimumLength(13).WithMessage("Invalid card number");

            RuleFor(x => x.CardInfo!.ExpirationMonth)
                .InclusiveBetween(1, 12).WithMessage("Invalid expiration month");

            RuleFor(x => x.CardInfo!.ExpirationYear)
                .GreaterThanOrEqualTo(DateTime.UtcNow.Year).WithMessage("Card is expired");
        });
    }
}
