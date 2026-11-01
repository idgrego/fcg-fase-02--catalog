using FCG02.CatalogAPI.Domain.Enums;

namespace FCG02.CatalogAPI.Application.DTOs;

public record GameUpdateRequest(
    string Title,
    string Description,
    decimal Price,
    GameCategory Category,
    List<string>? Tags = null
);