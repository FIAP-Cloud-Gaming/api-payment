namespace Payment.Infrastructure.IntegrationEvents;

public class IntegrationEventLogService(PaymentContext context) 
    : IIntegrationEventLogService
{
    public async Task<IEnumerable<IntegrationEventLogEntry>> RetrieveEventLogsPendingToPublishAsync(Guid transactionId)
    {
        var result = await context.IntegrationEventLogs
            .Where(e => e.TransactionId == transactionId && e.State == EventStateEnum.NotPublished)
            .ToListAsync();

        if (result.Count != 0)
        {
            return result.OrderBy(o => o.CreationTime)
                .Select(e => e);
        }

        return [];
    }

    public async Task SaveEventAsync(object @event, Guid transactionId)
    {
        var eventLogEntry = new IntegrationEventLogEntry(@event, transactionId);

        await context.IntegrationEventLogs.AddAsync(eventLogEntry);
        await context.SaveChangesAsync();
    }

    public async Task MarkEventAsPublishedAsync(Guid eventId)
    {
        await UpdateEventStatus(eventId, EventStateEnum.Published);
    }

    public async Task MarkEventAsInProgressAsync(Guid eventId)
    {
        await UpdateEventStatus(eventId, EventStateEnum.InProgress);
    }

    public async Task MarkEventAsFailedAsync(Guid eventId)
    {
        await UpdateEventStatus(eventId, EventStateEnum.PublishedFailed);
    }

    private async Task UpdateEventStatus(Guid eventId, EventStateEnum status)
    {
        var eventLogEntry = await context.IntegrationEventLogs.SingleAsync(ie => ie.EventId == eventId);
        eventLogEntry.State = status;

        if (status == EventStateEnum.InProgress)
            eventLogEntry.TimesSent++;

        await context.SaveChangesAsync();
    }
}
