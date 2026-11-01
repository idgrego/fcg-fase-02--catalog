using FCG02.CatalogAPI.Application.DTOs;

namespace FCG02.CatalogAPI.Application.Interfaces;

public interface IOrderService
{
    Task<OrderResponse> PlaceOrderAsync(Guid userId, OrderCreateRequest request, CancellationToken cancellationToken = default);
}