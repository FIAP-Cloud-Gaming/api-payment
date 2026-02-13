namespace Payment.API.Application.IntegrationEvents;

public interface IPaymentIntegrationEventService
{
    Task PublishEventsThroughEventBusAsync(Guid transactionId);
    Task AddAndSaveEventAsync(object @event);
}
