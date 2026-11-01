# Stage 1: construção e publicação dos artefatos
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia os arquivos .csproj preservando a hierarquia de diretórios
# (Permite que o Docker utilize o cache de camadas para o restore do NuGet)
COPY src/FCG02.CatalogAPI.Domain/FCG02.CatalogAPI.Domain.csproj src/FCG02.CatalogAPI.Domain/
COPY src/FCG02.CatalogAPI.Application/FCG02.CatalogAPI.Application.csproj src/FCG02.CatalogAPI.Application/
COPY src/FCG02.CatalogAPI.Infrastructure/FCG02.CatalogAPI.Infrastructure.csproj src/FCG02.CatalogAPI.Infrastructure/
COPY src/FCG02.CatalogAPI.API/FCG02.CatalogAPI.API.csproj src/FCG02.CatalogAPI.API/

# Restaura as dependências externas da API e projetos referênciados
RUN dotnet restore src/FCG02.CatalogAPI.API/FCG02.CatalogAPI.API.csproj

# Copía todo o código fonte restante
COPY src/ src/

# Compila e gera os binários otimizados de produção
RUN dotnet publish src/FCG02.CatalogAPI.API/FCG02.CatalogAPI.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: gera a imagem final de runtime (mínima e segura)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# ativa o usuário não root nativo do .NET
USER app

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Copia estritamente os artefatos compilados do estágio anterior
COPY --from=build /app/publish .

ENTRYPOINT [ "dotnet", "FCG02.CatalogAPI.API.dll" ]