namespace Payment.Infrastructure.IntegrationEvents.Events;

public record PaymentCancelledIntegrationEvent
{
    public Guid PaymentId { get; init; }
    public string OrderId { get; init; } = default!;
    public string UserId { get; init; } = default!;
    public string Reason { get; init; } = default!;
    public DateTime CancelledAt { get; init; }
}
