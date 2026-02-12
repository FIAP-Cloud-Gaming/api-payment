namespace Payment.Infrastructure.IntegrationEvents.EventHandling;

public class PaymentRefundedDomainEventHandler(
    IEventBus eventBus,
    ILogger<PaymentRefundedDomainEventHandler> logger)
    : INotificationHandler<PaymentRefundedDomainEvent>
{
    public async Task Handle(PaymentRefundedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Payment {PaymentId} refunded for Order {OrderId} - Reason: {Reason}",
            domainEvent.Payment.Id.Value,
            domainEvent.Payment.OrderId,
            domainEvent.Reason);

        var integrationEvent = new PaymentRefundedIntegrationEvent
        {
            PaymentId = domainEvent.Payment.Id.Value,
            OrderId = domainEvent.Payment.OrderId,
            UserId = domainEvent.Payment.UserId,
            RefundAmount = domainEvent.Payment.TotalAmount.Value,
            Currency = domainEvent.Payment.TotalAmount.Currency,
            Reason = domainEvent.Reason,
            RefundedAt = domainEvent.Payment.UpdatedAt!.Value
        };

        await eventBus.PublishAsync(integrationEvent);
    }
}
