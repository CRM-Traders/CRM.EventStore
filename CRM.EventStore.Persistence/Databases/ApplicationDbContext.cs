using System.Reflection;
using CRM.EventStore.Domain.Entities.StoredEvents;
using Microsoft.EntityFrameworkCore;

namespace CRM.EventStore.Persistence.Databases;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Event> Events => Set<Event>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
