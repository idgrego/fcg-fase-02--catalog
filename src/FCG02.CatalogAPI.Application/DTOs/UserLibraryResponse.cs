using FCG02.CatalogAPI.Domain.Entities;

namespace FCG02.CatalogAPI.Application.DTOs;

public record UserLibraryResponse(
    Guid UserId,
    IReadOnlyCollection<LibraryItemResponse> Items,
    DateTime CreatedAt,
    DateTime? UpdatedAt
)
{
    public static UserLibraryResponse FromEntity(UserLibrary library) => new UserLibraryResponse(
        UserId: library.UserId,
        Items: library.Items.Select(LibraryItemResponse.FromEntitty).ToList(),
        CreatedAt: library.CreatedAt,
        UpdatedAt: library.UpdatedAt
    );
}