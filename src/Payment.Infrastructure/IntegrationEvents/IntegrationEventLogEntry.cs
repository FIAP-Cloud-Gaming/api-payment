using System.Text.Json;

namespace Payment.Infrastructure.IntegrationEvents;

public class IntegrationEventLogEntry
{
    public Guid EventId { get; private set; }
    public string EventTypeName { get; private set; } = null!;
    public string Content { get; private set; } = null!;
    public DateTime CreationTime { get; private set; }
    public EventStateEnum State { get; set; }
    public int TimesSent { get; set; }
    public Guid TransactionId { get; private set; }

    public IntegrationEventLogEntry(object @event, Guid transactionId)
    {
        EventId = Guid.NewGuid();
        CreationTime = DateTime.UtcNow;
        EventTypeName = @event.GetType().FullName!;
        Content = JsonSerializer.Serialize(@event, @event.GetType());
        State = EventStateEnum.NotPublished;
        TimesSent = 0;
        TransactionId = transactionId;
    }

    private IntegrationEventLogEntry() { }
}

public enum EventStateEnum
{
    NotPublished = 0,
    InProgress = 1,
    Published = 2,
    PublishedFailed = 3
}
