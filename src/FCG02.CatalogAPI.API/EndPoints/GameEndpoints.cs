using FCG02.CatalogAPI.Application.DTOs;
using FCG02.CatalogAPI.Application.Interfaces;
using FCG02.CatalogAPI.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace FCG02.CatalogAPI.API.EndPoints;

public static class GameEndPoints
{
    public static IEndpointRouteBuilder MapGameEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/games")
            .WithTags("Games");

        DefineGetAllGames(group);
        DefineGetGameById(group);
        DefineCreateGame(group);
        DefineUpdateGame(group);
        DefineDeleteGame(group);

        return app;
    }

    /// <summary>
    /// Define a consulta pública de jogos com filtros opcionais
    /// </summary>
    /// <param name="group"></param>
    /// <exception cref="NotImplementedException"></exception>
    private static void DefineGetAllGames(RouteGroupBuilder group)
    {
        group.MapGet("/", async (
            [FromQuery] GameCategory? category,
            [FromQuery] bool onlyActive,
            IGameService gameService,
            CancellationToken cancellationToken
        ) =>
        {
            var games = await gameService.GetAllAsync(category, onlyActive, cancellationToken);
            return Results.Ok(games);
        })
        .AllowAnonymous()
        .WithName("GetAllGames")
        .WithSummary("Consulta pública do catálogo de jogos")
        .WithDescription("Retorna a lista de jogos disponíveis com a opção de filtro por categoria e status ativo.")
        .Produces<IReadOnlyList<GameResponse>>(StatusCodes.Status200OK);
    }

    /// <summary>
    /// Consulta pública de detalhes de 1 jogo
    /// </summary>
    /// <param name="group"></param>
    private static void DefineGetGameById(RouteGroupBuilder group)
    {
        group.MapGet("/{id:guid}", async (
            Guid id,
            IGameService gameService,
            CancellationToken cancellationToken
        ) =>
        {
            var game = await gameService.GetByIdAsync(id, cancellationToken);
            return game is not null
                ? Results.Ok(game)
                : Results.NotFound(new { message = $"Game '{id}' was not found." });
        })
        .AllowAnonymous()
        .WithName("GetGameById")
        .WithSummary("Consulta detalhes de um jogo")
        .Produces<GameResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }

    /// <summary>
    /// Define a rota para criação de um jogo
    /// </summary>
    /// <param name="group"></param>
    /// <exception cref="NotImplementedException"></exception>
    private static void DefineCreateGame(RouteGroupBuilder group)
    {
        group.MapPost("/", async (
            [FromBody] GameCreateRequest request,
            IGameService gameService,
            CancellationToken cancellationToken
        ) =>
        {
            try
            {
                var createdGame = await gameService.CreateAsync(request, cancellationToken);
                return Results.Created($"/api/v1/games/{createdGame.Id}", createdGame);
            }
            catch (System.ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("CreateGame")
        .WithSummary("Catastra um novo jogo")
        .WithDescription("Permite que um administrador realize o cadastro de um novo jogo no catálogo.")
        .Produces<GameResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }

    private static void DefineUpdateGame(RouteGroupBuilder group)
    {
        group.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] GameUpdateRequest request,
            IGameService gameService,
            CancellationToken cancellationToken
        ) =>
        {
            try
            {

                var updatedGame = await gameService.UpdateAsync(id, request, cancellationToken);
                return updatedGame is not null
                    ? Results.Ok(updatedGame)
                    : Results.NotFound(new { message = $"Game '{id}' was not found." });
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("Update Game")
        .WithSummary("Atualiza dados de um jogo")
        .WithDescription("Permite que um administrador atualize as informações de um jogo")
        .Produces<GameResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }

    /// <summary>
    /// Executa a inativação de um jogo
    /// </summary>
    /// <param name="group"></param>
    private static void DefineDeleteGame(RouteGroupBuilder group)
    {
        group.MapDelete("/{id:guid}", async (
            Guid id,
            IGameService gameService,
            CancellationToken cancellationToken
        ) =>
        {
            var success = await gameService.DeleteAsync(id, cancellationToken);
            return success
                ? Results.NoContent()
                : Results.NotFound(new { message = $"Game '{id}' was not found." });
        })
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("DeleteGame")
        .WithSummary("Inativa um jogo do catálogo")
        .WithDescription("Permie que um administrador inative um jogo")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }

}