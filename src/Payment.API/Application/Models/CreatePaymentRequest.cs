namespace Payment.API.Application.Models;

public sealed record CreatePaymentRequest
{
    public required string OrderId { get; init; }
    public required string UserId { get; init; }
    public required PaymentMethod Method { get; init; }
    public required IReadOnlyCollection<PaymentItemDto> Items { get; init; }
    public CardInfoDto? CardInfo { get; init; }
}

public sealed record PaymentItemDto
{
    public required string ProductId { get; init; }
    public required string ProductName { get; init; }
    public required int Quantity { get; init; }
    public required decimal UnitPriceAmount { get; init; }
    public string UnitPriceCurrency { get; init; } = "BRL";
}

public sealed record CardInfoDto
{
    public required string CardHolderName { get; init; }
    public required string CardNumber { get; init; }
    public required string CardBrand { get; init; }
    public required int ExpirationMonth { get; init; }
    public required int ExpirationYear { get; init; }
}
