using FCG02.CatalogAPI.Application.DTOs;

namespace FCG02.CatalogAPI.Application.Interfaces;

public interface IUserLibraryService
{
    Task<UserLibraryResponse> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddGameToLibraryASync(Guid userId, Guid gameId, decimal pricePaid, CancellationToken cancellationToken = default);
}