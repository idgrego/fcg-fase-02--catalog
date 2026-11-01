using FCG02.CatalogAPI.Domain.Entities;

namespace FCG02.CatalogAPI.Domain.Interfaces;

public interface IUserLibraryRepository
{
    Task<UserLibrary?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(UserLibrary library, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserLibrary library, CancellationToken cancellationToken = default);
}