using System.Security.Claims;
using FCG02.CatalogAPI.Application.DTOs;
using FCG02.CatalogAPI.Application.Interfaces;
using Microsoft.VisualBasic;

namespace FCG02.CatalogAPI.API.EndPoints;

public static class LibraryEndpoints
{
    public static IEndpointRouteBuilder MapLibraryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/library")
            .WithTags("Library")
            .RequireAuthorization();

        group.MapGet("/", async (
            ClaimsPrincipal user,
            IUserLibraryService libraryService,
            CancellationToken cancellationToken
        ) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Results.Unauthorized();

            var library = await libraryService.GetByUserIdAsync(userId, cancellationToken);
            return Results.Ok(library);
        })
        .WithName("GetUserLibrary")
        .WithSummary("Consulta a biblioteca de jogos adquiridos do jogador autenticado")
        .Produces<UserLibraryResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);

        return app;
    }
}