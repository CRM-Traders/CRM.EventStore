using CRM.EventStore.Application.Common.Persistence.Repositories;
using CRM.EventStore.Domain.Entities.StoredEvents;
using CRM.EventStore.Persistence.Databases;
using Microsoft.EntityFrameworkCore;

namespace CRM.EventStore.Persistence.Repositories;

public class EventRepository : IEventRepository
{
    private readonly ApplicationDbContext _dbContext;

    public EventRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Event?> GetEventByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Events.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<Event> AddEventAsync(Event @event, CancellationToken cancellationToken = default)
    {
        await _dbContext.Events.AddAsync(@event, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return @event;
    }

    public async Task<IReadOnlyList<Event>> GetEventsByAggregateIdAsync(Guid aggregateId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Events
            .Where(e => e.AggregateId == aggregateId)
            .OrderBy(e => e.OccurredOn)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Event>> GetEventsByTypeAsync(string eventType, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Events
            .Where(e => e.EventType == eventType)
            .OrderByDescending(e => e.OccurredOn)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Event>> GetEventsByServiceNameAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Events
            .Where(e => e.ServiceName == serviceName)
            .OrderByDescending(e => e.OccurredOn)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Event>> GetEventsByDateRangeAsync(DateTimeOffset startDate, DateTimeOffset endDate, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Events
            .Where(e => e.OccurredOn >= startDate && e.OccurredOn <= endDate)
            .OrderByDescending(e => e.OccurredOn)
            .ToListAsync(cancellationToken);
    }
}