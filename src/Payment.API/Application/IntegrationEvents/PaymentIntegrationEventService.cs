using Payment.Infrastructure.IntegrationEvents;
using System.Text.Json;

namespace Payment.API.Application.IntegrationEvents;

public class PaymentIntegrationEventService(
    PaymentContext context,
    IEventBus eventBus,
    IIntegrationEventLogService eventLogService,
    ILogger<PaymentIntegrationEventService> logger) : IPaymentIntegrationEventService
{
    public async Task PublishEventsThroughEventBusAsync(Guid transactionId)
    {
        var pendingLogEvents = await eventLogService.RetrieveEventLogsPendingToPublishAsync(transactionId);

        foreach (var logEvt in pendingLogEvents)
        {
            logger.LogInformation("Publishing integration event: {IntegrationEventId} - ({@IntegrationEvent})", logEvt.EventId, logEvt);

            try
            {
                await eventLogService.MarkEventAsInProgressAsync(logEvt.EventId);

                var eventType = Type.GetType(logEvt.EventTypeName)
                    ?? throw new InvalidOperationException($"Could not resolve type '{logEvt.EventTypeName}'");

                var integrationEvent = JsonSerializer.Deserialize(logEvt.Content, eventType) as IntegrationEvent
                    ?? throw new InvalidOperationException($"Could not deserialize event '{logEvt.EventTypeName}'");

                await eventBus.PublishAsync(integrationEvent);

                await eventLogService.MarkEventAsPublishedAsync(logEvt.EventId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error publishing integration event: {IntegrationEventId}", logEvt.EventId);
                await eventLogService.MarkEventAsFailedAsync(logEvt.EventId);
            }
        }
    }

    public async Task AddAndSaveEventAsync(object @event)
    {
        logger.LogInformation("Enqueuing integration event {IntegrationEventId} to repository ({@IntegrationEvent})", @event.GetType().Name, @event);

        var transaction = context.GetCurrentTransaction()
            ?? throw new InvalidOperationException("A transaction must be active to save integration events");

        await eventLogService.SaveEventAsync(@event, transaction.TransactionId);
    }
}
