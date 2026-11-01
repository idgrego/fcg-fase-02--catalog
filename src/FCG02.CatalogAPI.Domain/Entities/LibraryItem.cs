namespace FCG02.CatalogAPI.Domain.Entities;

public record LibraryItem
(
    Guid GameId,
    string GameTitle,
    decimal PricePaid,
    DateTime AcquiredAt
);