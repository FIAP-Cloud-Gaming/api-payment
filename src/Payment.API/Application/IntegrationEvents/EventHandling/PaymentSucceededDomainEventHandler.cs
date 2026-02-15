namespace Payment.API.Application.IntegrationEvents.EventHandling;

public class PaymentSucceededDomainEventHandler(
    IPaymentIntegrationEventService integrationEventService,
    ILogger<PaymentSucceededDomainEventHandler> logger)
    : INotificationHandler<PaymentSucceededDomainEvent>
{
    public async Task Handle(PaymentSucceededDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Payment {PaymentId} succeeded for Order {OrderId} - TransactionId: {TransactionId}",
            domainEvent.Payment.Id.Value,
            domainEvent.Payment.OrderId,
            domainEvent.Payment.Details.TransactionId);

        var integrationEvent = new PaymentSucceededIntegrationEvent
        {
            PaymentId = domainEvent.Payment.Id.Value,
            OrderId = domainEvent.Payment.OrderId,
            UserId = domainEvent.Payment.UserId,
            TotalAmount = domainEvent.Payment.TotalAmount.Value,
            Currency = domainEvent.Payment.TotalAmount.Currency,
            TransactionId = domainEvent.Payment.Details.TransactionId!,
            AuthorizationCode = domainEvent.Payment.Details.AuthorizationCode,
            ProcessedAt = domainEvent.Payment.ProcessedAt!.Value
        };

        await integrationEventService.AddAndSaveEventAsync(integrationEvent);
    }
}
