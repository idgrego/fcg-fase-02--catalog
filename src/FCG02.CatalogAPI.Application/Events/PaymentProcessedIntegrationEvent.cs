using System.Globalization;

namespace FCG02.CatalogAPI.Application.Events;

/// <summary>
/// Contrato do evento de integração publnicado pela PaymentsAPI ao concluir uma transação
/// </summary>
/// <param name="EventId"></param>
/// <param name="OccurredOn"></param>
/// <param name="OrderId"></param>
/// <param name="UserId"></param>
/// <param name="GameId"></param>
/// <param name="Amount"></param>
/// <param name="Status"></param>
/// <param name="Reason"></param>
/// <param name="ProcessedAt"></param>
public record PaymentProcessedIntegrationEvent(
    Guid EventId,
    DateTime OccurredOn,
    Guid OrderId,
    Guid UserId,
    Guid GameId,
    decimal Amount,
    string Status,
    string? Reason,
    DateTime ProcessedAt
)
{
    /// <summary>
    /// Propriedade utilitária para compatibilidade caso o payload envie "Price" em vez de "Amount"
    /// </summary>
    public decimal Price => Amount;

    public bool IsApproved => string.Equals(Status, "Approved", StringComparison.OrdinalIgnoreCase);
};