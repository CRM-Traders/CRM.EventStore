using System.Text.Json.Serialization;

namespace CRM.EventStore.Application.Common.Messages;

public class EventMessage
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("eventType")]
    public string EventType { get; set; } = string.Empty;

    [JsonPropertyName("serviceName")]
    public string ServiceName { get; set; } = string.Empty;

    [JsonPropertyName("aggregateId")]
    public Guid AggregateId { get; set; }

    [JsonPropertyName("aggregateType")]
    public string AggregateType { get; set; } = string.Empty;

    [JsonPropertyName("occurredOn")]
    public DateTimeOffset OccurredOn { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("metadata")]
    public string? Metadata { get; set; }
}