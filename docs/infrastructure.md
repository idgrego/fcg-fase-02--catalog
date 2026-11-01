# CatalogAPI - Documentação Técnica: Camada de Infraestrutura

A camada **`FCG02.CatalogAPI.Infrastructure`** provê a implementação técnica concreta das abstrações definidas no Domínio e na Aplicação, gerenciando a persistência relacional no **Azure Database** e a mensageria assíncrona com **RabbitMQ 7.x**.

---

## 📑 Sumário

- [1. Visão Geral](#1-visão-geral)
- [2. Persistência Relacional com Azure Database (`CatalogDbContext`)](#2-persistência-relacional-com-azure-database-catalogdbcontext)
  - [2.1. Mapeamento de Entidades via Fluent API](#21-mapeamento-de-entidades-via-fluent-api)
  - [2.2. Precisão Financeira (`HasPrecision`)](#22-precisão-financeira-hasprecision)
  - [2.3. Criação Automática de Índices](#23-criação-automática-de-índices)
- [3. Repositórios](#3-repositórios)
  - [3.1. `GameRepository`](#31-gamerepository)
  - [3.2. `UserLibraryRepository`](#32-userlibraryrepository)
- [4. Mensageria Assíncrona com RabbitMQ](#4-mensageria-assíncrona-com-rabbitmq)
  - [4.1. Publicador: `RabbitMqEventPublisher`](#41-publicador-rabbitmqeventpublisher)
  - [4.2. Consumidor em Segundo Plano: `PaymentProcessedConsumerService`](#42-consumidor-em-segundo-plano-paymentprocessedconsumerservice)
- [5. Injeção de Dependências da Infraestrutura](#5-injeção-de-dependências-da-infraestrutura)
- [6. Navegação](#6-navegação)

---

## 1. Visão Geral

A infraestrutura é completamente desacoplada das regras puras de negócio. Caso o provedor relacional ou o broker fossem substituídos, apenas esta camada sofreria alterações, mantendo Domínio e Aplicação 100% intactos.

---

## 2. Persistência Relacional com Azure Database (`CatalogDbContext`)

Localização: `src/FCG02.CatalogAPI.Infrastructure/Data/CatalogDbContext.cs`

### 2.1. Mapeamento de Entidades via Fluent API
No método `OnModelCreating` do contexto, registramos as restrições:
- Mapeamento explícito da entidade `Game` com limites de tamanho de string (ex: `HasMaxLength(150)`).
- Mapeamento da coleção privada `_items` da entidade `UserLibrary` utilizando Owned Entities (`OwnsMany`). O EF Core gerencia a coleção como a tabela associada `LibraryItems`, respeitando o encapsulamento.
- Conversão serializada em JSON para SQL Server para persistir a lista de strings (Tags) do `Game`.

### 2.2. Precisão Financeira (`HasPrecision`)
Para evitar erros de arredondamento inerentes a números de ponto flutuante, configuramos o preço do jogo com precisão:
```csharp
entity.Property(e => e.Price).HasPrecision(18, 2);
```

### 2.3. Criação Automática de Índices
Para melhorar o desempenho da base:
1. **Índice Único por Usuário:** `entity.HasIndex(e => e.UserId).IsUnique();` impedindo duplicidade de bibliotecas.
2. **Índice Composto de Catálogo:** `Category + IsActive` para aceleração imediata dos filtros da vitrine de jogos.

---

## 3. Repositórios

### 3.1. `GameRepository`
Localização: `src/FCG02.CatalogAPI.Infrastructure/Repositories/GameRepository.cs`
- Realiza consultas otimizadas utilizando LINQ.
- Suporta busca combinada com filtros.
- Modifica as entidades utilizando transações embutidas no `SaveChangesAsync`.

### 3.2. `UserLibraryRepository`
Localização: `src/FCG02.CatalogAPI.Infrastructure/Repositories/UserLibraryRepository.cs`
- Busca a biblioteca do usuário através do `UserId`, realizando um `.Include(ul => ul.Items)` para garantir que o agregado inteiro esteja carregado na memória.

---

## 4. Mensageria Assíncrona com RabbitMQ

### 4.1. Publicador: `RabbitMqEventPublisher`
Publica o evento `OrderPlacedEvent` na exchange `fcg.catalog.events`:
- Declara a Exchange do tipo `Topic` de forma idempotente (`durable: true`).
- Formata o payload em JSON UTF-8.
- Marca as propriedades com `Persistent: true` para garantir persistência em disco.

### 4.2. Consumidor em Segundo Plano: `PaymentProcessedConsumerService`
Opera como um `BackgroundService` contínuo:
- Escuta na fila `catalog.payment-processed` com binding pattern **`payment.*`**.
- Configura a leitura justa de mensagens.
- Ao receber a mensagem:
  - Se `isApproved == true`: Cria um escopo (`IServiceScopeFactory`) e invoca `_libService.AddGameToLibraryASync(...)`.
  - Se `isApproved == false`: Registra em log estruturado e descarta a alteração de biblioteca.
- Emite `BasicAckAsync` manual em ambos os casos (sucesso de negócio).

---

## 5. Injeção de Dependências da Infraestrutura

Método de extensão `AddInfrastructureServices` em `DependencyInjection.cs`:
- Registra `CatalogDbContext` apontando para o Azure Database via `Microsoft.EntityFrameworkCore.SqlServer` (`UseSqlServer` com `EnableRetryOnFailure`).
- Registra `IGameRepository` e `IUserLibraryRepository` como Scoped.
- Registra `IEventPublisher` como Singleton.
- Registra `PaymentProcessedConsumerService` como Hosted Service (`AddHostedService`).

---

## 6. Navegação

- ⬅️ **[Voltar para a Camada de Aplicação (`application.md`)](application.md)**
- ➡️ **[Avançar para a Camada de Apresentação (`api.md`)](api.md)**
