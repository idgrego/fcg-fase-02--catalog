
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FCG02.CatalogAPI.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace FCG02.CatalogAPI.Infrastructure.Messaging;

public class RabbitMqEventPublisher : IEventPublisher, IAsyncDisposable
{
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<RabbitMqEventPublisher> _logger;
    private readonly ConnectionFactory _factory;
    private IConnection? _connection;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);

    public RabbitMqEventPublisher(
        IOptions<RabbitMqSettings> options,
        ILogger<RabbitMqEventPublisher> logger
    )
    {
        _logger = logger;
        _settings = options.Value;
        _factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            Port = _settings.Port,
            UserName = _settings.UserName,
            Password = _settings.Password,
            AutomaticRecoveryEnabled = true
        };
    }

    private async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken)
    {
        if (_connection is not null && _connection.IsOpen)
            return _connection;

        await _connectionLock.WaitAsync(cancellationToken);

        try
        {
            _logger.LogInformation("Conectando ao RabbitMQ em {Host}:{Port}...", _settings.HostName, _settings.Port);
            _connection = await _factory.CreateConnectionAsync(cancellationToken);
            return _connection;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    private static string ResolveRoutingKey(string eventName) =>
        eventName switch
        {
            "OrderPlacedEvent" => "order.placed",
            _ => "catalog.generic"
        };

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class
    {
        try
        {
            var connection = await GetConnectionAsync(cancellationToken);
            await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
            await channel.ExchangeDeclareAsync(
                exchange: _settings.Exchange,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                cancellationToken: cancellationToken
            );

            var eventName = typeof(TEvent).Name;
            var routingKey = ResolveRoutingKey(eventName);

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() }
            };
            var payload = JsonSerializer.Serialize(@event, jsonOptions);
            var body = Encoding.UTF8.GetBytes(payload);

            var properties = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent,
                Type = eventName,
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            };

            await channel.BasicPublishAsync(
                exchange: _settings.Exchange,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);

            _logger.LogInformation("🚀 [RabbitMQ] Evento '{EventName}' publicado com sucesso na exchange '{Exchange}' (RoutingKey: '{RoutingKey}')",
                eventName, _settings.Exchange, routingKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [RabbitMQ] Falha ao publicar evento '{EventName}'.", typeof(TEvent).Name);
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
            _connection = null;
        }
        _connectionLock.Dispose();
        GC.SuppressFinalize(this);
    }
}