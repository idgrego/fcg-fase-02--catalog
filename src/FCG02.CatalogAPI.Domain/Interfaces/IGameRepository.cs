using FCG02.CatalogAPI.Domain.Entities;
using FCG02.CatalogAPI.Domain.Enums;

namespace FCG02.CatalogAPI.Domain.Interfaces;

public interface IGameRepository
{
    Task<Game?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Game>> GetAllAsync(GameCategory? category = null, bool onlyActive = true, CancellationToken cancellationToken = default);
    Task AddAsync(Game game, CancellationToken cancellationToken = default);
    Task UpdateAsync(Game game, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}