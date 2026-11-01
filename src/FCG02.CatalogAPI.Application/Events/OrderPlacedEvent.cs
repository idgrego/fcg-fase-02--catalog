namespace FCG02.CatalogAPI.Application.Events;

public record OrderPlacedEvent(
    Guid EventId,
    DateTime OccurredOn,
    Guid OrderId,
    Guid UserId,
    Guid GameId,
    decimal Price
);