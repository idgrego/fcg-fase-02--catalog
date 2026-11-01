using FCG02.CatalogAPI.Domain.Entities;

namespace FCG02.CatalogAPI.Application.DTOs;

public record LibraryItemResponse(
    Guid GameId,
    string GameTitle,
    decimal PricePaid,
    DateTime AcquiredAt
)
{
    public static LibraryItemResponse FromEntitty(LibraryItem item) => new LibraryItemResponse(
        GameId: item.GameId,
        GameTitle: item.GameTitle,
        PricePaid: item.PricePaid,
        AcquiredAt: item.AcquiredAt
    );
}