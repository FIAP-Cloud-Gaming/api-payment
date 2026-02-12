namespace Payment.Infrastructure.IntegrationEvents.EventHandling;

public class PaymentCancelledDomainEventHandler(
    IEventBus eventBus,
    ILogger<PaymentCancelledDomainEventHandler> logger)
    : INotificationHandler<PaymentCancelledDomainEvent>
{
    public async Task Handle(PaymentCancelledDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Payment {PaymentId} cancelled for Order {OrderId} - Reason: {Reason}",
            domainEvent.Payment.Id.Value,
            domainEvent.Payment.OrderId,
            domainEvent.Reason);

        var integrationEvent = new PaymentCancelledIntegrationEvent
        {
            PaymentId = domainEvent.Payment.Id.Value,
            OrderId = domainEvent.Payment.OrderId,
            UserId = domainEvent.Payment.UserId,
            Reason = domainEvent.Reason,
            CancelledAt = domainEvent.Payment.ProcessedAt!.Value
        };

        await eventBus.PublishAsync(integrationEvent);
    }
}
