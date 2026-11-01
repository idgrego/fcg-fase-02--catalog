using FCG02.CatalogAPI.Domain.Entities;
using FCG02.CatalogAPI.Domain.Enums;

namespace FCG02.CatalogAPI.Application.DTOs;

public record GameResponse(
    Guid Id,
    string Title,
    string Description,
    decimal Price,
    GameCategory Category,
    IReadOnlyCollection<string> Tags,
    bool IsActive,
    DateTime CreateAt,
    DateTime? UpdatedAt
)
{
    public static GameResponse FromEntity(Game game) => new GameResponse(
        Id: game.Id,
        Title: game.Title,
        Description: game.Description,
        Price: game.Price,
        Category: game.Category,
        Tags: game.Tags,
        IsActive: game.IsActive,
        CreateAt: game.CreatedAt,
        UpdatedAt: game.UpdatedAt
    );
}