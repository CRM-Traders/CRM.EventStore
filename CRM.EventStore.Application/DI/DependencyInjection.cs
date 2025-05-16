using System.Reflection;
using CRM.EventStore.Application.Common;
using CRM.EventStore.Application.Common.Abstractions.Mediators;
using CRM.EventStore.Application.Common.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CRM.EventStore.Application.DI;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddScoped<IMediator, Mediator>();
        services.AddValidatorsFromAssembly(assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        RegisterHandlers(services, assembly);

        return services;
    }


    private static void RegisterHandlers(IServiceCollection services, Assembly assembly)
    {
        var handlerType = typeof(IRequestHandler<,>);

        var handlers = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .Where(t => t.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerType));

        foreach (var handler in handlers)
        {
            var interfaces = handler.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerType);

            foreach (var interfaceType in interfaces)
            {
                services.AddTransient(interfaceType, handler);
            }
        }
    }
}

