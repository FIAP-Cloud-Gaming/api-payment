namespace Payment.Infrastructure.IntegrationEvents.Events;

public record PaymentStartedIntegrationEvent
{
    public Guid PaymentId { get; init; }
    public string OrderId { get; init; } = default!;
    public string UserId { get; init; } = default!;
    public decimal TotalAmount { get; init; }
    public string Currency { get; init; } = default!;
    public string PaymentMethod { get; init; } = default!;
    public DateTime CreatedAt { get; init; }
}
