using FCG02.CatalogAPI.Domain.Entities;
using FCG02.CatalogAPI.Domain.Interfaces;
using FCG02.CatalogAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FCG02.CatalogAPI.Infrastructure.Repositories;

public class UserLibraryRepository : IUserLibraryRepository
{
    private readonly CatalogDbContext _context;

    public UserLibraryRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(UserLibrary library, CancellationToken cancellationToken = default)
    {
        await _context.UserLibraries.AddAsync(library, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserLibrary?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserLibraries
            .Include(ul => ul.Items)
            .FirstOrDefaultAsync(i => i.UserId == userId, cancellationToken);
    }

    public async Task UpdateAsync(UserLibrary library, CancellationToken cancellationToken = default)
    {
        _context.UserLibraries.Update(library);
        await _context.SaveChangesAsync(cancellationToken);
    }
}