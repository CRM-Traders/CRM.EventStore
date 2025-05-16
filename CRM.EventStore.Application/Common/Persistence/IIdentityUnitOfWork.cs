namespace CRM.EventStore.Application.Common.Persistence;

public interface IIdentityUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}