using CRM.EventStore.Application.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class EventConsumerHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventConsumerHostedService> _logger;
    private IEventConsumer _eventConsumer;

    public EventConsumerHostedService(
        IServiceProvider serviceProvider,
        ILogger<EventConsumerHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Event Consumer Service starting");

        _eventConsumer = _serviceProvider.GetRequiredService<IEventConsumer>();
        _eventConsumer.StartConsuming();

        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Event Consumer Service stopping");

        if (_eventConsumer != null)
        {
            await _eventConsumer.StopConsumingAsync();

            if (_eventConsumer is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        await base.StopAsync(cancellationToken);
    }
}