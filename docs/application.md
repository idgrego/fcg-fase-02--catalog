# CatalogAPI - Documentação Técnica: Camada de Aplicação

A camada **`FCG02.CatalogAPI.Application`** orquestra os casos de uso do microsserviço de catálogo, mediando a interação entre os endpoints HTTP (camada de apresentação), as regras de negócio puras (camada de domínio), a persistência relacional no Azure Database e a mensageria assíncrona.

---

## 📑 Sumário

- [1. Estrutura e Responsabilidades](#1-estrutura-e-responsabilidades)
- [2. DTOs (Data Transfer Objects)](#2-dtos-data-transfer-objects)
- [3. Contratos de Eventos de Integração](#3-contratos-de-eventos-de-integração)
- [4. Casos de Uso e Serviços de Aplicação](#4-casos-de-uso-e-serviços-de-aplicação)
  - [4.1. `GameService`: Catálogo e Gestão de Jogos](#41-gameservice-catálogo-e-gestão-de-jogos)
  - [4.2. `OrderService`: Iniciação de Pedidos e Publicação Assíncrona](#42-orderservice-iniciação-de-pedidos-e-publicação-assíncrona)
  - [4.3. `UserLibraryService`: Adição e Consulta de Biblioteca](#43-userlibraryservice-adição-e-consulta-de-biblioteca)
- [5. Interfaces de Suporte](#5-interfaces-de-suporte)
- [6. Navegação](#6-navegação)

---

## 1. Estrutura e Responsabilidades

A camada de aplicação não acessa diretamente o banco de dados nem bibliotecas de rede; ela depende exclusivamente de abstrações:
- Utiliza **DTOs imutáveis (`record`)** para transporte de dados, evitando expor entidades de domínio diretamente na API.
- Orquestra transações lógicas e garante que um jogo só seja comprado caso exista, esteja ativo e ainda não pertença ao usuário.
- Converte entidades para respostas de API via métodos de extensão ou construtores de mapeamento limpos.

---

## 2. DTOs (Data Transfer Objects)

Localização: `src/FCG02.CatalogAPI.Application/DTOs/`

- **`GameCreateRequest`:** Dados para cadastro de um novo jogo (`Title`, `Description`, `Price`, `Category`, `Tags`).
- **`GameUpdateRequest`:** Dados para atualização cadastral de um jogo existente.
- **`GameResponse`:** Representação pública do jogo para consumidores da API.
- **`OrderCreateRequest`:** Requisição de compra contendo apenas o `GameId` (o `UserId` é extraído de forma segura das claims do token JWT).
- **`OrderResponse`:** Resposta com status `PendingPayment` e o identificador do pedido gerado (`OrderId`).
- **`UserLibraryResponse` & `LibraryItemResponse`:** Lista estruturada de jogos adquiridos pelo jogador com data de aquisição e preço pago.

---

## 3. Contratos de Eventos de Integração

Localização: `src/FCG02.CatalogAPI.Application/Events/`

### 3.1. `OrderPlacedEvent` (Evento Publicado)
Publicado na Topic Exchange `fcg.catalog.events` com a chave `order.placed`:
```csharp
public record OrderPlacedEvent(
    Guid EventId,
    DateTime OccurredOn,
    Guid OrderId,
    Guid UserId,
    Guid GameId,
    decimal Price
);
```

### 3.2. `PaymentProcessedIntegrationEvent` (Evento Consumido)
Recebido da fila `catalog.payment-processed` vinculada ao Exchange `fcg.payments.events`:
```csharp
public record PaymentProcessedIntegrationEvent(
    Guid EventId,
    DateTime OccurredOn,
    Guid PaymentId,
    Guid OrderId,
    Guid UserId,
    Guid GameId,
    decimal Amount,
    string Status,
    string? Reason,
    DateTime ProcessedAt
)
{
    public bool IsApproved => string.Equals(Status, "Approved", StringComparison.OrdinalIgnoreCase);
}
```

---

## 4. Casos de Uso e Serviços de Aplicação

### 4.1. `GameService`: Catálogo e Gestão de Jogos
- `GetByIdAsync`: Recupera os detalhes de um jogo por Id.
- `GetAllAsync`: Lista jogos com filtro opcional por `GameCategory`.
- `CreateAsync`: Instancia a entidade `Game` (gerando UUIDv7) e persiste no Azure Database.
- `UpdateAsync`: Atualiza detalhes mantendo invariantes protegidas.
- `DeleteAsync`: Desativa logicamente o jogo através do método `Deactivate()`.

### 4.2. `OrderService`: Iniciação de Pedidos e Publicação Assíncrona
Valida se o usuário pode comprar o jogo antes de emitir o pedido:
1. Verifica se o `GameId` existe e está ativo.
2. Consulta a biblioteca do jogador para checar se ele já possui o jogo (`lib.HasGame(game.Id)`).
3. Cria o evento `OrderPlacedEvent` com UUIDv7.
4. Despacha o evento assíncrono para o RabbitMQ via `IEventPublisher`.
5. Retorna imediatamente o `OrderResponse` com `Status: "PendingPayment"`.

### 4.3. `UserLibraryService`: Adição e Consulta de Biblioteca
- `GetLibraryByUserIdAsync`: Retorna todos os jogos adquiridos pelo jogador autenticado.
- `AddGameToLibraryASync`: Invocado pelo consumidor em segundo plano (`PaymentProcessedConsumerService`) ao receber a confirmação de pagamento:
  - Recupera ou cria a biblioteca do usuário no Azure Database.
  - Adiciona o jogo com valor pago e carimbo de data/hora UTC.
  - Persiste a atualização de forma idempotente.

---

## 5. Interfaces de Suporte

Localização: `src/FCG02.CatalogAPI.Application/Interfaces/`

- **`IGameService`:** Contrato para manipulação e consulta do catálogo.
- **`IOrderService`:** Contrato para processamento e iniciação de pedidos de compra.
- **`IUserLibraryService`:** Contrato para gerenciamento da biblioteca do jogador.
- **`IEventPublisher`:** Abstração de saída para envio de eventos ao RabbitMQ sem acoplamento à biblioteca concreta de mensageria.

---

## 6. Navegação

- ⬅️ **[Voltar para a Camada de Domínio (`domain.md`)](domain.md)**
- ➡️ **[Avançar para a Camada de Infraestrutura (`infrastructure.md`)](infrastructure.md)**
