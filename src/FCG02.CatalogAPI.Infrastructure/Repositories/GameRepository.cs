using FCG02.CatalogAPI.Domain.Entities;
using FCG02.CatalogAPI.Domain.Enums;
using FCG02.CatalogAPI.Domain.Interfaces;
using FCG02.CatalogAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FCG02.CatalogAPI.Infrastructure.Repositories;

public class GameRepository : IGameRepository
{
    private readonly CatalogDbContext _context;
    
    public GameRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Game>> GetAllAsync(GameCategory? category = null, bool onlyActive = true, CancellationToken cancellationToken = default)
    {
        var query = _context.Games.AsQueryable();

        if (onlyActive)
            query = query.Where(g => g.IsActive);
        
        if (category.HasValue)
            query = query.Where(g => g.Category == category.Value);

        return await query
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync(cancellationToken);
    }

    public async Task<Game?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Games
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public async Task AddAsync(Game game, CancellationToken cancellationToken = default)
    {
        await _context.Games.AddAsync(game, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var game = await _context.Games.FindAsync([id], cancellationToken: cancellationToken);
        if (game != null)
        {
            _context.Games.Remove(game);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task UpdateAsync(Game game, CancellationToken cancellationToken = default)
    {
        _context.Games.Update(game);
        await _context.SaveChangesAsync(cancellationToken);
    }
}