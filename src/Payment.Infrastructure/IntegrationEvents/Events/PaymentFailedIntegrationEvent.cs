namespace Payment.Infrastructure.IntegrationEvents.Events;

/// <summary>
/// Integration event published when a payment fails
/// </summary>
public record PaymentFailedIntegrationEvent
{
    public Guid PaymentId { get; init; }
    public string OrderId { get; init; } = default!;
    public string UserId { get; init; } = default!;
    public string FailureReason { get; init; } = default!;
    public string? ProcessorResponse { get; init; }
    public DateTime FailedAt { get; init; }
}
