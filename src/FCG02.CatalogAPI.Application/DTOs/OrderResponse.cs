namespace FCG02.CatalogAPI.Application.DTOs;

public record OrderResponse(
    Guid OrderId,
    Guid UserId,
    Guid GameId,
    string GameTitle,
    decimal Price,
    string Status,
    DateTime CreatedAt
);