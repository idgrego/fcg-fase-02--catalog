# CatalogAPI - Documentação Técnica: Suíte de Testes Unitários

A **CatalogAPI** possui uma suíte abrangente de testes automatizados construída com **xUnit**, **Moq** e asserções expressivas, garantindo a integridade comportamental das regras de domínio e dos fluxos de aplicação.

---

## 📑 Sumário

- [1. Estratégia de Testes](#1-estratégia-de-testes)
- [2. Testes de Domínio (`Domain`)](#2-testes-de-domínio-domain)
  - [2.1. `GameTests`](#21-gametests)
  - [2.2. `UserLibraryTests`](#22-userlibrarytests)
- [3. Testes de Aplicação (`Application`)](#3-testes-de-aplicação-application)
  - [3.1. `GameServiceTests`](#31-gameservicetests)
  - [3.2. `OrderServiceTests`](#32-orderservicetests)
  - [3.3. `PaymentProcessedHandlingTests`](#33-paymentprocessedhandlingtests)
- [4. Como Executar os Testes via Terminal](#4-como-executar-os-testes-via-terminal)
- [5. Navegação](#5-navegação)

---

## 1. Estratégia de Testes

Os testes seguem rigorosamente o padrão **AAA (Arrange, Act, Assert)**:
- **Testes de Domínio:** Executados sem nenhum mock ou framework externo, validando puramente as regras de negócio das entidades em memória.
- **Testes de Aplicação:** Utilizam o **Moq** para criar dublês determinísticos dos repositórios (`IGameRepository`, `IUserLibraryRepository`) e do publicador de eventos (`IEventPublisher`), testando cenários de sucesso e caminhos de exceção.

---

## 2. Testes de Domínio (`Domain`)

Localização: `tests/FCG02.CatalogAPI.UnitTests/Domain/`

### 2.1. `GameTests`
- Valida que instanciar um `Game` com dados válidos define `IsActive = true`, gera `Id` válido (UUIDv7) e registra `CreatedAt`.
- Rejeita títulos nulos, vazios ou com mais de 150 caracteres.
- Rejeita descrições nulas, vazias ou com mais de 2000 caracteres.
- Rejeita preços negativos (`price < 0`) e arredonda valores monetários para duas casas decimais.
- Valida o comportamento de ativação (`Activate()`) e desativação (`Deactivate()`) atualizando o `UpdatedAt`.

### 2.2. `UserLibraryTests`
- Valida a criação de biblioteca associada a um `UserId`.
- Valida que adicionar um jogo válido inclui o item na coleção `Items`.
- **Proteção contra duplicidade:** Garante que tentar adicionar um jogo já existente na biblioteca lança `InvalidOperationException`.

---

## 3. Testes de Aplicação (`Application`)

Localização: `tests/FCG02.CatalogAPI.UnitTests/Application/`

### 3.1. `GameServiceTests`
- Valida o fluxo de criação persistindo a nova entidade no repositório simulado.
- Valida a atualização e exclusão lógica de títulos.

### 3.2. `OrderServiceTests`
- **Sucesso:** Ao solicitar a compra de um jogo ativo e ainda não possuído, o serviço publica o `OrderPlacedEvent` no RabbitMQ e retorna `OrderResponse` com `PendingPayment`.
- **Jogo Inativo/Inexistente:** Garante o lançamento de `InvalidOperationException` sem publicar nenhum evento.
- **Jogo Já Adquirido:** Valida que tentar comprar um jogo que já está na biblioteca do jogador é barrado antes do envio para o broker.

### 3.3. `PaymentProcessedHandlingTests`
- Valida o processamento de confirmação de pagamento:
  - Se `Status == "Approved"`: Adiciona o jogo à biblioteca do usuário.
  - Se `Status == "Rejected"`: Não adiciona o jogo e finaliza a mensagem com sucesso sem corromper o estado do banco.

---

## 4. Como Executar os Testes via Terminal

No terminal, a partir da raiz do repositório ou da pasta `fcg-fase-02--catalog`:

```powershell
dotnet test tests/FCG02.CatalogAPI.UnitTests/FCG02.CatalogAPI.UnitTests.csproj --logger "console;verbosity=detailed"
```

Todos os **20 testes unitários** devem passar com sucesso total (0 falhas).

---

## 5. Navegação

- ⬅️ **[Voltar para a Apresentação (`api.md`)](api.md)**
- ➡️ **[Avançar para a Containerização (`docker.md`)](docker.md)**
