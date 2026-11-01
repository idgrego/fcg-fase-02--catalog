# CatalogAPI - Documentação Técnica: Camada de Apresentação (API)

A camada **`FCG02.CatalogAPI.API`** expõe os endpoints HTTP da aplicação através de **Minimal APIs modulares**, protegidas por autenticação JWT Bearer e documentadas interativamente com **Scalar**.

---

## 📑 Sumário

- [1. Visão Geral das Minimal APIs](#1-visão-geral-das-minimal-apis)
- [2. Configuração de Segurança e JWT no `Program.cs`](#2-configuração-de-segurança-e-jwt-no-programcs)
- [3. Mapeamento de Rotas por Contexto de Negócio](#3-mapeamento-de-rotas-por-contexto-de-negócio)
  - [3.1. `GameEndpoints` (Catálogo Público & CRUD Admin)](#31-gameendpoints-catálogo-público--crud-admin)
  - [3.2. `OrderEndpoints` (Iniciação de Compra)](#32-orderendpoints-iniciação-de-compra)
  - [3.3. `LibraryEndpoints` (Biblioteca do Jogador)](#33-libraryendpoints-biblioteca-do-jogador)
  - [3.4. `HealthCheck`](#34-healthcheck)
- [4. Documentação Interativa com Scalar](#4-documentação-interativa-com-scalar)
- [5. Navegação](#5-navegação)

---

## 1. Visão Geral das Minimal APIs

A CatalogAPI utiliza o padrão moderno de Minimal APIs do .NET 10:
- **Alta Performance:** Elimina o peso do pipeline tradicional de Controllers do MVC (menor alocação de memória e inicialização acelerada).
- **Modularização Limpa:** Em vez de poluir o arquivo `Program.cs` com dezenas de rotas, os endpoints são agrupados em classes de extensão estáticas (`GameEndpoints.cs`, `OrderEndpoints.cs`, `LibraryEndpoints.cs`).
- **Segurança Nativa:** Aplicação de políticas com `.RequireAuthorization("Admin")` ou `.RequireAuthorization()` nos grupos de rotas com `MapGroup`.

---

## 2. Configuração de Segurança e JWT no `Program.cs`

A CatalogAPI valida tokens emitidos pela `UsersAPI` de forma completamente autônoma (*stateless*):
- **`TokenValidationParameters`:**
  - `ValidateIssuerSigningKey = true`: Validação da assinatura com `SymmetricSecurityKey`.
  - `ValidateIssuer = true`: Confirma que o emissor é `FCG02.UsersAPI`.
  - `ValidateAudience = true`: Confirma que a audiência é `FCG02.CloudGamesClients`.
  - `ValidateLifetime = true` com `ClockSkew = TimeSpan.Zero`: Expiração imediata sem margem de tolerância.

---

## 3. Mapeamento de Rotas por Contexto de Negócio

### 3.1. `GameEndpoints` (Catálogo Público & CRUD Admin)
Rota base: `/api/v1/games`

| Método | Rota | Descrição | Permissão | Respostas |
|---|---|---|---|---|
| `GET` | `/api/v1/games` | Lista jogos com filtro por categoria (`?category=Action`) | Pública | `200 OK` |
| `GET` | `/api/v1/games/{id}` | Retorna detalhes do jogo por Id | Pública | `200 OK`, `404 Not Found` |
| `POST` | `/api/v1/games` | Cadastra novo jogo | `Admin` | `201 Created`, `400 Bad Request`, `401 Unauthorized`, `403 Forbidden` |
| `PUT` | `/api/v1/games/{id}` | Atualiza dados e preço do jogo | `Admin` | `200 OK`, `400 Bad Request`, `404 Not Found` |
| `DELETE` | `/api/v1/games/{id}` | Desativa logicamente um jogo | `Admin` | `204 No Content`, `404 Not Found` |

### 3.2. `OrderEndpoints` (Iniciação de Compra)
Rota base: `/api/v1/orders`

| Método | Rota | Descrição | Permissão | Respostas |
|---|---|---|---|---|
| `POST` | `/api/v1/orders` | Inicia o pedido e publica evento no RabbitMQ | `Gamer` / `Admin` | `202 Accepted`, `400 Bad Request`, `401 Unauthorized` |

> 🔒 **Extração Segura do Usuário:** O endpoint extrai o identificador do jogador diretamente da claim `ClaimTypes.NameIdentifier` do token JWT autenticado:
> ```csharp
> var userId = Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
> ```

### 3.3. `LibraryEndpoints` (Biblioteca do Jogador)
Rota base: `/api/v1/library`

| Método | Rota | Descrição | Permissão | Respostas |
|---|---|---|---|---|
| `GET` | `/api/v1/library` | Retorna a biblioteca do usuário logado | `Gamer` / `Admin` | `200 OK`, `401 Unauthorized` |

### 3.4. `HealthCheck`
- `GET /health`: Endpoint público para monitoramento de contêineres e orquestradores (Kubernetes / Docker Compose), retornando `{ "status": "Health", "service": "CatalogAPI" }`.

---

## 4. Documentação Interativa com Scalar

Em ambiente de desenvolvimento (`IsDevelopment()`), a especificação OpenAPI é exposta nativamente em `/openapi/v1.json` e renderizada na interface moderna do **Scalar**:

👉 **URL de Acesso:** `http://localhost:8082/scalar/v1`

A documentação permite a exploração completa dos schemas, autenticação interativa com token Bearer e envio direto de requisições de teste.

---

## 5. Navegação

- ⬅️ **[Voltar para a Camada de Infraestrutura (`infrastructure.md`)](infrastructure.md)**
- ➡️ **[Avançar para a Suíte de Testes Unitários (`tests.md`)](tests.md)**
