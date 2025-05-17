namespace CRM.EventStore.Application.Common.Services;

public interface IEventConsumer
{
    void StartConsuming();
    Task StopConsumingAsync();
}