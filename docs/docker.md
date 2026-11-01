# CatalogAPI - Documentação Técnica: Containerização com Docker

Este documento detalha o processo de empacotamento, segurança e orquestração em contêineres do microsserviço **CatalogAPI** (`fcg-fase-02--catalog`) em **.NET 10**.

---

## 📑 Sumário

- [1. Estratégia de Multi-Stage Build](#1-estratégia-de-multi-stage-build)
- [2. Segurança e Execução Não-Root (`USER app`)](#2-segurança-e-execução-não-root-user-app)
- [3. Otimização do Cache de Camadas (*Layer Caching*)](#3-otimização-do-cache-de-camadas-layer-caching)
- [4. Arquivo `.dockerignore`](#4-arquivo-dockerignore)
- [5. O `Dockerfile` Completo](#5-o-dockerfile-completo)
- [6. Integração ao Docker Compose](#6-integração-ao-docker-compose)
- [7. Navegação](#7-navegação)

---

## 1. Estratégia de Multi-Stage Build

A CatalogAPI utiliza **Multi-Stage Build** para manter os contêineres finais de produção extremamente leves e seguros:
- **Estágio de Build (`build`):** Utiliza a imagem completa `mcr.microsoft.com/dotnet/sdk:10.0` com todos os utilitários de compilação, restore de NuGet e MSBuild.
- **Estágio de Runtime (`final`):** Utiliza a imagem enxuta `mcr.microsoft.com/dotnet/aspnet:10.0` contendo apenas os binários de execução do ASP.NET Core, descartando compiladores e ferramentas pesadas.

---

## 2. Segurança e Execução Não-Root (`USER app`)

- Tradicionalmente, contêineres executavam sob o usuário `root` (UID 0), representando risco de segurança caso houvesse vulnerabilidade de escape do contêiner.
- No .NET 10, adotamos a diretiva **`USER app`** (UID 1654), que roda a aplicação sem privilégios administrativos.
- A porta interna configurada é a **`8080`** (`ASPNETCORE_URLS=http://+:8080`), uma porta não-privilegiada permitida para usuários comuns no Linux.

---

## 3. Otimização do Cache de Camadas (*Layer Caching*)

No `Dockerfile`, copiamos **apenas os arquivos `.csproj`** antes do comando `dotnet restore`. Dessa forma, o Docker armazena as dependências do NuGet em cache, recompilando as camadas de código em poucos segundos quando apenas arquivos `.cs` são alterados.

---

## 4. Arquivo `.dockerignore`

Localização: `fcg-fase-02--catalog/.dockerignore`

```dockerignore
**/.git
**/.vs
**/.vscode
**/bin
**/obj
**/*.user
**/*.suo
**/Dockerfile*
README.md
docs/
```

---

## 5. O `Dockerfile` Completo

Localização: `fcg-fase-02--catalog/Dockerfile`

```dockerfile
# Stage 1: construção e publicação dos artefatos
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia os arquivos .csproj preservando a hierarquia de diretórios
COPY src/FCG02.CatalogAPI.Domain/FCG02.CatalogAPI.Domain.csproj src/FCG02.CatalogAPI.Domain/
COPY src/FCG02.CatalogAPI.Application/FCG02.CatalogAPI.Application.csproj src/FCG02.CatalogAPI.Application/
COPY src/FCG02.CatalogAPI.Infrastructure/FCG02.CatalogAPI.Infrastructure.csproj src/FCG02.CatalogAPI.Infrastructure/
COPY src/FCG02.CatalogAPI.API/FCG02.CatalogAPI.API.csproj src/FCG02.CatalogAPI.API/

# Restaura as dependências externas a partir do projeto de entrada (API)
RUN dotnet restore src/FCG02.CatalogAPI.API/FCG02.CatalogAPI.API.csproj

# Copia todo o código-fonte restante
COPY src/ src/

# Compila e gera os binários de produção
RUN dotnet publish src/FCG02.CatalogAPI.API/FCG02.CatalogAPI.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: imagem final de runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Ativa o usuário não root nativo do .NET
USER app

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Copia estritamente os artefatos compilados do estágio anterior
COPY --from=build /app/publish .

ENTRYPOINT [ "dotnet", "FCG02.CatalogAPI.API.dll" ]
```

---

## 6. Integração ao Docker Compose

No arquivo `fcg-fase-02--orchestration/docker-compose.yml`:
- Mapeado para a porta host **`8082:8080`**.
- Conectado à rede virtual **`fcg-network`**.
- Conexão com o banco dedicado `fcg-02-catalogdb` no Azure Database (`fcg-server.database.windows.net`).
- Dependência de inicialização segura com `depends_on`:
  - `rabbitmq`: `condition: service_healthy`

---

## 7. Navegação

- ⬅️ **[Voltar para os Testes Unitários (`tests.md`)](tests.md)**
- 📑 **[Voltar para o Índice Central da Documentação (`index.md`)](index.md)**
