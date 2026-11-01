using FCG02.CatalogAPI.Application.Interfaces;
using FCG02.CatalogAPI.Domain.Interfaces;
using FCG02.CatalogAPI.Infrastructure.Data;
using FCG02.CatalogAPI.Infrastructure.Messaging;
using FCG02.CatalogAPI.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FCG02.CatalogAPI.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // configurações tipadas
        services.Configure<RabbitMqSettings>(configuration.GetSection("RabbitMQ"));

        // contexto do SQL Server
        services.AddDbContext<CatalogDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"), sqlOptions => sqlOptions.EnableRetryOnFailure()));

        // repositórios
        services.AddScoped<IGameRepository, GameRepository>();
        services.AddScoped<IUserLibraryRepository, UserLibraryRepository>();

        // Mensageria
        services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();
        services.AddHostedService<PaymentProcessedConsumerService>();

        return services;
    }
}