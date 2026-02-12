namespace Payment.Infrastructure.IntegrationEvents.EventHandling;

public class PaymentStartedDomainEventHandler(
    IEventBus eventBus,
    ILogger<PaymentStartedDomainEventHandler> logger)
    : INotificationHandler<PaymentStartedDomainEvent>
{
    public async Task Handle(PaymentStartedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Payment {PaymentId} started for Order {OrderId}",
            domainEvent.Payment.Id.Value,
            domainEvent.Payment.OrderId);

        var integrationEvent = new PaymentStartedIntegrationEvent
        {
            PaymentId = domainEvent.Payment.Id.Value,
            OrderId = domainEvent.Payment.OrderId,
            UserId = domainEvent.Payment.UserId,
            TotalAmount = domainEvent.Payment.TotalAmount.Value,
            Currency = domainEvent.Payment.TotalAmount.Currency,
            PaymentMethod = domainEvent.Payment.Method.ToString(),
            CreatedAt = domainEvent.Payment.CreatedAt
        };

        await eventBus.PublishAsync(integrationEvent);
    }
}
