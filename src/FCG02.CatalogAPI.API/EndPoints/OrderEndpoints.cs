using System.Security.Claims;
using FCG02.CatalogAPI.Application.DTOs;
using FCG02.CatalogAPI.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FCG02.CatalogAPI.API.EndPoints;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {

        var group = app.MapGroup("/api/v1/orders")
            .WithTags("Orders")
            .RequireAuthorization();

        // inicia pedido de compra para o jogador autenticado
        group.MapPost("/", async (
            [FromBody] OrderCreateRequest request,
            ClaimsPrincipal user,
            IOrderService orderService,
            CancellationToken cancellationToken
        ) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Results.Unauthorized();

            try
            {
                var order = await orderService.PlaceOrderAsync(userId, request, cancellationToken);
                return Results.Accepted($"/api/v1/orders/{order.OrderId}", order);

            }
            catch (System.ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("PlaceOrder")
        .WithSummary("Inicia solicitação de compra de um jogo")
        .WithDescription("Valida o jogo e a biblioteca do jogador e dispara o evento OrderPlacedEvent no RabbitMQ")
        .Produces<OrderResponse>(StatusCodes.Status202Accepted)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        return app;
    }
}