namespace Payment.API.Application.Models;

public sealed record PaymentDto
{
    public required string Id { get; init; }
    public required string OrderId { get; init; }
    public required string UserId { get; init; }
    public required decimal TotalAmount { get; init; }
    public required string Currency { get; init; }
    public required PaymentStatus Status { get; init; }
    public required PaymentMethod Method { get; init; }
    public CardInfoDto? CardInfo { get; init; }
    public string? TransactionId { get; init; }
    public string? AuthorizationCode { get; init; }
    public string? FailureReason { get; init; }
    public required IReadOnlyCollection<PaymentItemDto> Items { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? ProcessedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }

    public static PaymentDto FromPayment(Domain.AggregatesModel.PaymentAggregate.Payment payment)
    {
        ArgumentNullException.ThrowIfNull(payment);

        return new PaymentDto
        {
            Id = payment.Id.ToString(),
            OrderId = payment.OrderId,
            UserId = payment.UserId,
            TotalAmount = payment.TotalAmount.Value,
            Currency = payment.TotalAmount.Currency,
            Status = payment.Status,
            Method = payment.Method,
            CardInfo = payment.CardInfo != null ? new CardInfoDto
            {
                CardHolderName = payment.CardInfo.CardHolderName,
                CardNumber = payment.CardInfo.MaskedCardNumber,
                CardBrand = payment.CardInfo.CardBrand,
                ExpirationMonth = payment.CardInfo.ExpirationMonth,
                ExpirationYear = payment.CardInfo.ExpirationYear
            } : null,
            TransactionId = payment.Details.TransactionId,
            AuthorizationCode = payment.Details.AuthorizationCode,
            FailureReason = payment.Details.FailureReason,
            Items = payment.Items.Select(i => new PaymentItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPriceAmount = i.UnitPrice.Value,
                UnitPriceCurrency = i.UnitPrice.Currency
            }).ToList(),
            CreatedAt = payment.CreatedAt,
            ProcessedAt = payment.ProcessedAt,
            UpdatedAt = payment.UpdatedAt
        };
    }
}
