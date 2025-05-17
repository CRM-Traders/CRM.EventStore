using System.Text;
using System.Text.Json;
using CRM.EventStore.Application.Common.Messages;
using CRM.EventStore.Application.Common.Persistence.Repositories;
using CRM.EventStore.Application.Common.Services;
using CRM.EventStore.Domain.Common.Options.RabbitMq;
using CRM.EventStore.Domain.Entities.StoredEvents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CRM.EventStore.Infrastructure.Consumers;

public class RabbitMQEventConsumer : IEventConsumer, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly IEventRepository _eventRepository;
    private readonly ILogger<RabbitMQEventConsumer> _logger;
    private readonly RabbitMQOptions _options;

    public RabbitMQEventConsumer(
        IEventRepository eventRepository,
        IOptions<RabbitMQOptions> options,
        ILogger<RabbitMQEventConsumer> logger)
    {
        _eventRepository = eventRepository;
        _logger = logger;
        _options = options.Value;

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
        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (sender, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                _logger.LogInformation("Received message with routing key: {RoutingKey}", ea.RoutingKey);

                await ProcessEventMessageAsync(message);

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

    private async Task ProcessEventMessageAsync(string message)
    {
        try
        {
            var eventMessage = JsonSerializer.Deserialize<EventMessage>(message);

            if (eventMessage == null)
            {
                _logger.LogWarning("Failed to deserialize event message");
                return;
            }

            var storedEvent = new Event(
                eventMessage.Id,
                eventMessage.EventType,
                eventMessage.ServiceName,
                eventMessage.AggregateId,
                eventMessage.AggregateType,
                eventMessage.OccurredOn,
                eventMessage.Content,
                DateTimeOffset.UtcNow,
                eventMessage.Metadata);

            await _eventRepository.AddEventAsync(storedEvent);

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