using CRM.EventStore.Application.Common.Persistence;
using CRM.EventStore.Persistence.Databases;

namespace CRM.EventStore.Persistence.Utils;

public class IdentityUnitOfWork : IIdentityUnitOfWork
{
    private readonly IdentityDbContext _dbContext;

    public IdentityUnitOfWork(
        IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var result = await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}