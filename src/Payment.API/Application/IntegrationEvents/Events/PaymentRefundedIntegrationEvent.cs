namespace Payment.API.Application.IntegrationEvents.Events;

public record PaymentRefundedIntegrationEvent : IntegrationEvent
{
    public Guid PaymentId { get; init; }
    public string OrderId { get; init; } = default!;
    public string UserId { get; init; } = default!;
    public decimal RefundAmount { get; init; }
    public string Currency { get; init; } = default!;
    public string Reason { get; init; } = default!;
    public DateTime RefundedAt { get; init; }
}
