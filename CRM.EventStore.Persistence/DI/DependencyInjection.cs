using CRM.EventStore.Application.Common.Abstractions.Mediators;
using CRM.EventStore.Application.Common.Persistence;
using CRM.EventStore.Application.Common.Persistence.Repositories;
using CRM.EventStore.Persistence.Databases;
using CRM.EventStore.Persistence.Repositories;
using CRM.EventStore.Persistence.Repositories.Base;
using CRM.EventStore.Persistence.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CRM.EventStore.Persistence.DI;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddDbContext<IdentityDbContext>(options =>
        options.UseNpgsql(
                configuration.GetConnectionString("IdentityConnection"),
                b => b.MigrationsAssembly(typeof(IdentityDbContext).Assembly.FullName)));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped(typeof(IIdentityRepository<>), typeof(IdentityRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IIdentityUnitOfWork, IdentityUnitOfWork>();
        services.AddScoped<IEventRepository, EventRepository>();

        return services;
    }
}