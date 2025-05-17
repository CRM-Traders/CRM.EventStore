using CRM.EventStore.Domain.Entities.StoredEvents;

namespace CRM.EventStore.Application.Common.Persistence.Repositories;


public interface IEventRepository
{
    Task<Event?> GetEventByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Event> AddEventAsync(Event @event, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Event>> GetEventsByAggregateIdAsync(Guid aggregateId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Event>> GetEventsByTypeAsync(string eventType, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Event>> GetEventsByServiceNameAsync(string serviceName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Event>> GetEventsByDateRangeAsync(DateTimeOffset startDate, DateTimeOffset endDate, CancellationToken cancellationToken = default);
}