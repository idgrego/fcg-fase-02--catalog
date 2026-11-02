using FCG02.CatalogAPI.Application.DTOs;
using FCG02.CatalogAPI.Application.Events;
using FCG02.CatalogAPI.Application.Interfaces;
using FCG02.CatalogAPI.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace FCG02.CatalogAPI.Application.Services;

public class OrderService : IOrderService
{
    private readonly IGameRepository _gameRepository;
    private readonly IUserLibraryRepository _libRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<OrderService>? _logger;

    public OrderService(
        IGameRepository gameRepository,
        IUserLibraryRepository libRepository,
        IEventPublisher eventPublisher,
        ILogger<OrderService>? logger = null
    )
    {
        this._eventPublisher = eventPublisher;
        this._gameRepository = gameRepository;
        this._libRepository = libRepository;
        this._logger = logger;
    }

    public async Task<OrderResponse> PlaceOrderAsync(Guid userId, OrderCreateRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty.", nameof(userId));

        var game = await _gameRepository.GetByIdAsync(request.GameId, cancellationToken);
        if (game is null || !game.IsActive)
            throw new InvalidOperationException("Game not found or inactive for purchase.");

        _logger?.LogInformation("🎮 [CatalogAPI] Usuário {UserId} solicitou a compra do jogo '{GameTitle}' (R$ {Price:F2}).",
            userId, game.Title, game.Price);

        var lib = await _libRepository.GetByUserIdAsync(userId, cancellationToken);
        if (lib is not null && lib.HasGame(game.Id))
            throw new InvalidOperationException($"User already owns the game '{game.Title}'.");

        _logger?.LogInformation("✅ [CatalogAPI] Validação concluída: o usuário ainda não possui o jogo. Intenção de compra aprovada para envio.");

        var orderPlacedEvent = new OrderPlacedEvent(
            EventId: Guid.CreateVersion7(),
            OccurredOn: DateTime.UtcNow,
            OrderId: Guid.CreateVersion7(),
            UserId: userId,
            GameId: game.Id,
            Price: game.Price
        );

        await _eventPublisher.PublishAsync(orderPlacedEvent, cancellationToken);

        _logger?.LogInformation("🚀 [CatalogAPI] Objeto 'OrderPlacedEvent' (Pedido: {OrderId}) enviado para a fila de pagamentos. Deverá ser capturado pelo microsserviço de pagamentos. Tabela(s) modificada(s): Orders (intenção registrada).",
            orderPlacedEvent.OrderId);

        return new OrderResponse(
            OrderId: orderPlacedEvent.OrderId,
            UserId: userId,
            GameId: game.Id,
            GameTitle: game.Title,
            Price: game.Price,
            Status: "PendingPayment",
            CreatedAt: DateTime.UtcNow
        );
    }
}