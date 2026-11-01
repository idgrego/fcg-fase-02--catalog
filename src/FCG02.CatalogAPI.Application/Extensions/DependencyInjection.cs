using FCG02.CatalogAPI.Application.Interfaces;
using FCG02.CatalogAPI.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FCG02.CatalogAPI.Application.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        /*
        
  Projetos do tipo biblioteca (como é o caso FCG02.CatalogAPI.Application)  não incluem por padrão as referências do ASP.NET Core onde reside a interface IServiceCollection.
  A solução canônica e limpa em Clean Architecture é instalar o pacote oficial de abstrações de DI no projeto Application:

  📦 Execute no terminal:
   dotnet add src/FCG02.CatalogAPI.Application/FCG02.CatalogAPI.Application.csproj package Microsoft.Extensions.DependencyInjection.Abstractions
        
        */


        services.AddScoped<IGameService, GameService>();
        services.AddScoped<IUserLibraryService, UserLibraryService>();
        services.AddScoped<IOrderService, OrderService>();

        return services;
    }
}