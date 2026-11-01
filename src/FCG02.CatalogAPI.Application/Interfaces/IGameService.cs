using FCG02.CatalogAPI.Application.DTOs;
using FCG02.CatalogAPI.Domain.Enums;

namespace FCG02.CatalogAPI.Application.Interfaces;

public interface IGameService
{
    Task<IReadOnlyList<GameResponse>> GetAllAsync(GameCategory? category = null, bool onlyActive = true, CancellationToken cancellationToken = default);
    Task<GameResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<GameResponse> CreateAsync(GameCreateRequest request, CancellationToken cancellationToken = default);
    Task<GameResponse?> UpdateAsync(Guid id, GameUpdateRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}