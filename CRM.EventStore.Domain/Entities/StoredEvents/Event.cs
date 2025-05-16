using CRM.EventStore.Domain.Common.Entities;

namespace CRM.EventStore.Domain.Entities.StoredEvents;

public class Event : Entity
{
    public string EventType { get; private set; }
    public string ServiceName { get; private set; }
    public Guid AggregateId { get; private set; }
    public string AggregateType { get; private set; }
    public DateTimeOffset OccurredOn { get; private set; }
    public string Content { get; private set; }
    public string? Metadata { get; private set; }
    public DateTimeOffset ProcessedAt { get; private set; }

    public Event(
        Guid id,
        string eventType,
        string serviceName,
        Guid aggregateId,
        string aggregateType,
        DateTimeOffset occurredOn,
        string content,
        DateTimeOffset processedAt,
        string? metadata = null)
    {
        Id = id;
        EventType = eventType;
        ServiceName = serviceName;
        AggregateId = aggregateId;
        AggregateType = aggregateType;
        OccurredOn = occurredOn;
        Content = content;
        Metadata = metadata;
        ProcessedAt = processedAt;
    }
}