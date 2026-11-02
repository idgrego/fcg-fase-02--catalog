

using System.Globalization;
using System.Text;
using System.Text.Json;
using FCG02.CatalogAPI.Application.Events;
using FCG02.CatalogAPI.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FCG02.CatalogAPI.Infrastructure.Messaging;

public class PaymentProcessedConsumerService : BackgroundService
{
    private readonly RabbitMqSettings _settings;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PaymentProcessedConsumerService> _logger;
    private IConnection? _connection;
    private IChannel? _channel;

    public PaymentProcessedConsumerService(
        IOptions<RabbitMqSettings> options,
        IServiceScopeFactory scopeFactory,
        ILogger<PaymentProcessedConsumerService> logger
    )
    {
        _settings = options.Value;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogDebug("[RabbitMQ Worker] Inicializando consumidor de pagamentos em {Host}:{Port}...",
            _settings.HostName, _settings.Port);

        var factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            Port = _settings.Port,
            UserName = _settings.UserName,
            Password = _settings.Password,
            AutomaticRecoveryEnabled = true
        };

        try
        {
            _connection = await factory.CreateConnectionAsync(stoppingToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

            // declara a topic exchange de pagamentos (caso ainda não tenha sido criada pelo PaymentsAPI)
            await _channel.ExchangeDeclareAsync(
                exchange: _settings.PaymentsExchange,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                cancellationToken: stoppingToken
            );

            // declara a fila durável e exclusiva do catálogo
            await _channel.QueueDeclareAsync(
                queue: _settings.PaymentProcessedQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: stoppingToken
            );

            // vincula a fila à exchange como o padrão de roteamento (ex: payment.*)
            await _channel.QueueBindAsync(
                queue: _settings.PaymentProcessedQueue,
                exchange: _settings.PaymentsExchange,
                routingKey: _settings.PaymentProcessedRoutingKey,
                cancellationToken: stoppingToken
            );

            // configura o consumidor assíncrono com Confirmação Manual 
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += OnMessageReceivedAsync;

            await _channel.BasicConsumeAsync(
                queue: _settings.PaymentProcessedQueue,
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken
            );

            _logger.LogDebug("[RabbitMQ Worker] Consumidor ativo na fila '{Queue}' vinculada à exchange '{Exchange}' (RoutingKey: '{RoutingKey}')",
                _settings.PaymentProcessedQueue, _settings.PaymentsExchange, _settings.PaymentProcessedRoutingKey);

            // Mantém o worker ativo aguardando mensagens até o sinal de encerramento do host
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("🛑 [RabbitMQ Worker] Encerrando o consumidor de pagamentos...");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [RabbitMQ Worker] Falha crítica ao inicializar o consumidor de pagamentos.");
        }
    }

    private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs @event)
    {
        var rawBody = @event.Body.ToArray();
        var json = Encoding.UTF8.GetString(rawBody);

        try
        {
            _logger.LogDebug("[RabbitMQ Worker] Mensagem recebida! RoutingKey: '{RoutingKey}', DeliveryTag: {Tag}",
                @event.RoutingKey, @event.DeliveryTag);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var paymentEvent = JsonSerializer.Deserialize<PaymentProcessedIntegrationEvent>(json, options);
            if (paymentEvent is null)
            {
                _logger.LogWarning("[RabbitMQ Worker] Falha ao desserializar payload: {Json}", json);
                // Rejeita mensagem malformatada sem reenfileirar (evita loop infinito)
                await _channel!.BasicNackAsync(@event.DeliveryTag, multiple: false, requeue: false);
                return;
            }

            // verifica se o pagamento foi aprovado
            if (paymentEvent.IsApproved)
            {
                _logger.LogInformation("💳 [CatalogAPI] Evento de pagamento recebido para o Pedido {OrderId}: APROVADO (R$ {Amount:F2}). Adicionando jogo à biblioteca do usuário...",
                    paymentEvent.OrderId, paymentEvent.Amount);

                // cria o escopo para resolver o serviço Scoped de biblioteca
                using var scope = _scopeFactory.CreateScope();
                var libService = scope.ServiceProvider.GetRequiredService<IUserLibraryService>();

                // Adiciona o jogo à biblioteca do jogador no PostgreSQL
                await libService.AddGameToLibraryASync(
                    userId: paymentEvent.UserId,
                    gameId: paymentEvent.GameId,
                    pricePaid: paymentEvent.Amount,
                    cancellationToken: @event.CancellationToken
                );

                _logger.LogInformation("🎮 [CatalogAPI] Jogo {GameId} adicionado com sucesso à biblioteca do usuário {UserId}! Tabela(s) modificada(s): UserGames.",
                    paymentEvent.GameId, paymentEvent.UserId);
            }
            else
            {
                _logger.LogInformation("ℹ️ [CatalogAPI] Evento de pagamento recebido para o Pedido {OrderId}: REJEITADO. Motivo: '{Reason}'. Nenhuma tabela modificada.",
                    paymentEvent.OrderId, paymentEvent.Reason ?? "Não especificado");
            }

            // Confirma o processamento com sucesso removendo a mensagem da fila
            await _channel!.BasicAckAsync(@event.DeliveryTag, multiple: false, cancellationToken: @event.CancellationToken);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "[RabbitMQ Worker] Erro ao processar mensagem do evento de pagamento. Payload: {Json}", json);
            // Rejeita a mensagem. Em produção configuramos DLQ; por padrão não reenfileiramos para evitar starvation
            await _channel!.BasicNackAsync(@event.DeliveryTag, multiple: false, requeue: false);
        }


    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}