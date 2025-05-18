using System.Text;
using System.Text.Json;
using CRM.EventStore.Application.Common.Messages;
using CRM.EventStore.Application.Common.Persistence.Repositories;
using CRM.EventStore.Application.Common.Services;
using CRM.EventStore.Domain.Common.Options.RabbitMq;
using CRM.EventStore.Domain.Entities.StoredEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class RabbitMQEventConsumer : IEventConsumer, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMQEventConsumer> _logger;
    private readonly RabbitMQOptions _options;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public RabbitMQEventConsumer(
        IOptions<RabbitMQOptions> options,
        ILogger<RabbitMQEventConsumer> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _options = options.Value;
        _serviceScopeFactory = serviceScopeFactory;

        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            UserName = _options.UserName,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost,
            Port = _options.Port
        };

        try
        {
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(
                exchange: _options.ExchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false);

            _channel.QueueDeclare(
                queue: _options.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false);

            _channel.QueueBind(
                queue: _options.QueueName,
                exchange: _options.ExchangeName,
                routingKey: _options.RoutingKeyPattern);

            _logger.LogInformation("RabbitMQ connection established successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to establish RabbitMQ connection");
            throw;
        }
    }

    public void StartConsuming()
    {
        _channel.BasicQos(0, 1, false);

        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (sender, ea) =>
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();

            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                _logger.LogInformation("Received message with routing key: {RoutingKey}", ea.RoutingKey);

                await ProcessEventMessageAsync(message, eventRepository);

                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing RabbitMQ message");
                _channel.BasicNack(ea.DeliveryTag, false, true);
            }
        };

        _channel.BasicConsume(
            queue: _options.QueueName,
            autoAck: false,
            consumer: consumer);

        _logger.LogInformation("Started consuming messages from queue: {QueueName}", _options.QueueName);
    }

    public Task StopConsumingAsync()
    {
        _logger.LogInformation("Stopping RabbitMQ consumer");
        return Task.CompletedTask;
    }

    private async Task ProcessEventMessageAsync(string message, IEventRepository eventRepository)
    {
        try
        {
            var eventMessage = JsonSerializer.Deserialize<EventMessage>(message,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (eventMessage == null)
            {
                _logger.LogWarning("Failed to deserialize event message");
                return;
            }

            var content = eventMessage.Content;

            try
            {
                if (!content.StartsWith("{") && !content.StartsWith("["))
                {
                    content = JsonSerializer.Serialize(content);
                }
                else
                {
                    JsonDocument.Parse(content);
                }
            }
            catch
            {
                content = JsonSerializer.Serialize(content);
            }

            var storedEvent = new Event(
                eventMessage.Id,
                eventMessage.EventType,
                eventMessage.ServiceName,
                eventMessage.AggregateId,
                eventMessage.AggregateType,
                eventMessage.OccurredOn,
                content,
                DateTimeOffset.UtcNow,
                eventMessage.Metadata != null ? JsonSerializer.Serialize(eventMessage.Metadata) : null);

            await eventRepository.AddEventAsync(storedEvent);

            _logger.LogInformation("Stored event: {EventType} from service: {ServiceName} for aggregate: {AggregateId}",
                eventMessage.EventType, eventMessage.ServiceName, eventMessage.AggregateId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing event message");
            throw;
        }
    }
    public void Dispose()
    {
        try
        {
            _channel?.Close();
            _connection?.Close();
            _channel?.Dispose();
            _connection?.Dispose();

            _logger.LogInformation("RabbitMQ connection closed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing RabbitMQ consumer");
        }
    }
}