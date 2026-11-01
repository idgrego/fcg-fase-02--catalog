using System.ComponentModel;
using FCG02.CatalogAPI.Application.DTOs;
using FCG02.CatalogAPI.Application.Interfaces;
using FCG02.CatalogAPI.Domain.Entities;
using FCG02.CatalogAPI.Domain.Enums;
using FCG02.CatalogAPI.Domain.Interfaces;
using Microsoft.VisualBasic;

namespace FCG02.CatalogAPI.Application.Services;

public class GameService : IGameService
{
    private readonly IGameRepository _gameRepository;

    public GameService(IGameRepository gameRepository)
    {
        this._gameRepository = gameRepository;
    }

    public async Task<IReadOnlyList<GameResponse>> GetAllAsync(GameCategory? category = null, bool onlyActive = true, CancellationToken cancellationToken = default)
    {
        var games = await _gameRepository.GetAllAsync(category, onlyActive, cancellationToken);
        return games.Select(GameResponse.FromEntity).ToList();
    }

    public async Task<GameResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var game = await _gameRepository.GetByIdAsync(id, cancellationToken);
        return game is not null ? GameResponse.FromEntity(game) : null;
    }

    public async Task<GameResponse> CreateAsync(GameCreateRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));
        var game = new Game(request.Title, request.Description, request.Price, request.Category, request.Tags);
        await _gameRepository.AddAsync(game, cancellationToken);
        return GameResponse.FromEntity(game);
    }

    public async Task<GameResponse?> UpdateAsync(Guid id, GameUpdateRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var game = await _gameRepository.GetByIdAsync(id, cancellationToken);
        if (game is null) return null;

        game.UpdateDetails(request.Title, request.Description, request.Price, request.Category, request.Tags);
        await _gameRepository.UpdateAsync(game, cancellationToken);

        return GameResponse.FromEntity(game);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var game = await _gameRepository.GetByIdAsync(id, cancellationToken);
        if (game is null) return false;

        game.Deactivate();
        await _gameRepository.UpdateAsync(game, cancellationToken);
        return true;
    }
}