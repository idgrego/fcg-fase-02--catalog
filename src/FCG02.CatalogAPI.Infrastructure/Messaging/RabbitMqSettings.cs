namespace FCG02.CatalogAPI.Infrastructure.Messaging;

public class RabbitMqSettings
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";

    // Publicação de eventos do catálogo (ex: OrderPlacedEvent)
    public string Exchange { get; set; } = "fcg.catalog.events";

    // Consumo de eventos de pagamentos (PaymentProcessedEvent)
    public string PaymentsExchange { get; set; } = "fcg.payments.events";
    public string PaymentProcessedQueue { get; set; } = "catalog.payment-processed";
    public string PaymentProcessedRoutingKey { get; set; } = "payment.*";
}