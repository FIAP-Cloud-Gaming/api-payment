namespace Payment.Infrastructure.IntegrationEvents.EventHandling;

public class PaymentFailedDomainEventHandler(
    IEventBus eventBus,
    ILogger<PaymentFailedDomainEventHandler> logger)
    : INotificationHandler<PaymentFailedDomainEvent>
{
    public async Task Handle(PaymentFailedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogWarning(
            "Payment {PaymentId} failed for Order {OrderId} - Reason: {Reason}",
            domainEvent.Payment.Id.Value,
            domainEvent.Payment.OrderId,
            domainEvent.FailureReason);

        var integrationEvent = new PaymentFailedIntegrationEvent
        {
            PaymentId = domainEvent.Payment.Id.Value,
            OrderId = domainEvent.Payment.OrderId,
            UserId = domainEvent.Payment.UserId,
            FailureReason = domainEvent.FailureReason,
            ProcessorResponse = domainEvent.Payment.Details.ProcessorResponse,
            FailedAt = domainEvent.Payment.ProcessedAt!.Value
        };

        await eventBus.PublishAsync(integrationEvent);
    }
}
