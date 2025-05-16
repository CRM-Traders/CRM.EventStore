using CRM.EventStore.Domain.Entities.Permissions;
using Microsoft.EntityFrameworkCore;
namespace CRM.EventStore.Persistence.Databases;

public class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : DbContext(options)
{
    public DbSet<Permission> Permissions => Set<Permission>();
}
