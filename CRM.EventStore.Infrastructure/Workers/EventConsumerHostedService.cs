using CRM.EventStore.Application.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
namespace CRM.EventStore.Infrastructure.Workers;

public class EventConsumerHostedService(IServiceProvider _serviceProvider, ILogger<EventConsumerHostedService> _logger) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Event Consumer Service starting");

        var scope = _serviceProvider.CreateScope();
        var _eventConsumer = scope.ServiceProvider.GetRequiredService<IEventConsumer>();
        _eventConsumer.StartConsuming();

        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Event Consumer Service stopping");

        var scope = _serviceProvider.CreateScope();
        var _eventConsumer = scope.ServiceProvider.GetRequiredService<IEventConsumer>();
        await _eventConsumer.StopConsumingAsync();

        await base.StopAsync(cancellationToken);
    }
}