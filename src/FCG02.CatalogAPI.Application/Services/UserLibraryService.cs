using FCG02.CatalogAPI.Application.DTOs;
using FCG02.CatalogAPI.Application.Interfaces;
using FCG02.CatalogAPI.Domain.Interfaces;

namespace FCG02.CatalogAPI.Application.Services;

public class UserLibraryService : IUserLibraryService
{

    private readonly IUserLibraryRepository _libraryRepository;
    private readonly IGameRepository _gameRepository;

    public UserLibraryService(IUserLibraryRepository libraryRepository, IGameRepository gameRepository)
    {
        this._gameRepository = gameRepository;
        this._libraryRepository = libraryRepository;
    }

    public async Task<UserLibraryResponse> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var lib = await _libraryRepository.GetByUserIdAsync(userId, cancellationToken);
        if (lib is null)
        {
            // se o usuário ainda não possui retorna uma vazia
            return new UserLibraryResponse(
                userId,
                Array.Empty<LibraryItemResponse>(),
                DateTime.UtcNow,
                null
            );
        }

        return UserLibraryResponse.FromEntity(lib);
    }

    public async Task AddGameToLibraryASync(Guid userId, Guid gameId, decimal pricePaid, CancellationToken cancellationToken = default)
    {
        var game = await _gameRepository.GetByIdAsync(gameId, cancellationToken);
        if (game is null)
            throw new InvalidOperationException($"Game with ID '{gameId}' was not found");
        var lib = await _libraryRepository.GetByUserIdAsync(userId, cancellationToken);
        if (lib is null)
        {
            lib = new Domain.Entities.UserLibrary(userId);
            lib.AddGame(gameId, game.Title, pricePaid);
            await _libraryRepository.AddAsync(lib, cancellationToken);
        }
        else
        {
            if (!lib.HasGame(gameId))
            {
                lib.AddGame(gameId, game.Title, pricePaid);
                await _libraryRepository.UpdateAsync(lib, cancellationToken);
            }
        }
    }

}