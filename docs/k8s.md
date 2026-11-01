# CatalogAPI - Documentação Técnica: Orquestração com Kubernetes (K8s)

Este documento detalha a arquitetura de orquestração em nuvem da **CatalogAPI** (`fcg-fase-02--catalog`) em clusters **Kubernetes (K8s)**, atendendo aos padrões de governança, persistência relacional isolada com Azure Database e mensageria assíncrona da Fase 02.

---

## 📑 Sumário

- [1. Visão Geral da Topologia no Cluster](#1-visão-geral-da-topologia-no-cluster)
- [2. Estrutura de Manifestos (`/k8s`)](#2-estrutura-de-manifestos-k8s)
- [3. Segregação de Configurações: ConfigMap vs Secret](#3-segregação-de-configurações-configmap-vs-secret)
  - [3.1. `ConfigMap` (Configurações Públicas)](#31-configmap-configurações-públicas)
  - [3.2. `Secret` (Credenciais em Base64)](#32-secret-credenciais-em-base64)
- [4. Especificação do `Deployment`](#4-especificação-do-deployment)
  - [4.1. Réplicas e Alta Disponibilidade](#41-réplicas-e-alta-disponibilidade)
  - [4.2. Injeção de Variáveis via `envFrom`](#42-injeção-de-variáveis-via-envfrom)
  - [4.3. Sondas de Integridade (Readiness e Liveness Probes)](#43-sondas-de-integridade-readiness-e-liveness-probes)
  - [4.4. Limites e Requisições de Recursos (Resources)](#44-limites-e-requisições-de-recursos-resources)
- [5. Exposição de Rede com `Service` (ClusterIP)](#5-exposição-de-rede-com-service-clusterip)
- [6. Comandos Essenciais de Operação (`kubectl`)](#6-comandos-essenciais-de-operação-kubectl)
- [7. Navegação](#7-navegação)

---

## 1. Visão Geral da Topologia no Cluster

A **CatalogAPI** opera como um serviço autônomo com persistência relacional dedicada no Azure Database e comunicação orientada a eventos:

```mermaid
graph TD
    Client["🌐 Ingress / Frontend / Clientes"] -->|http://catalog-api-service:80| Svc["🌐 Service: catalog-api-service<br/>(ClusterIP - Porta 80)"]

    subgraph PodsGroup ["Pods Gerenciados pelo Deployment (Replicas: 2)"]
        Svc -->|Round-Robin| Pod1["📦 Pod: catalog-api-xxxx-1<br/>API :8080 | Consumer Ativo"]
        Svc -->|Round-Robin| Pod2["📦 Pod: catalog-api-xxxx-2<br/>API :8080 | Consumer Ativo"]
    end

    subgraph Configurations ["Injeção via envFrom"]
        CM["📋 ConfigMap: catalog-api-config<br/>(Exchanges, Queues)"]
        Sec["🔐 Secret: catalog-api-secret<br/>(Azure Database ConnString, Rabbit Pass, JWT Key)"]
    end

    CM -.-> Pod1
    CM -.-> Pod2
    Sec -.-> Pod1
    Sec -.-> Pod2
```

---

## 2. Estrutura de Manifestos (`/k8s`)

| Arquivo | Kind (Tipo) | Responsabilidade |
|---|---|---|
| **`01-configmap.yaml`** | `ConfigMap` | Configurações de exchanges e filas |
| **`02-secret.yaml`** | `Secret` | Connection string do Azure Database (fcg-02-catalogdb), senha do broker e chave JWT em Base64 |
| **`03-deployment.yaml`** | `Deployment` | Réplicas (2), injeção `envFrom`, probes `/health` e recursos |
| **`04-service.yaml`** | `Service` | Endpoint interno estável (ClusterIP) na porta 80 |

---

## 3. Segregação de Configurações: ConfigMap vs Secret

### 3.1. `ConfigMap` (Configurações Públicas)
- `ASPNETCORE_ENVIRONMENT`: `"Production"`
- `RabbitMQ__HostName`: `"rabbitmq-service"`
- `RabbitMQ__Port`: `"5672"`
- `RabbitMQ__UserName`: `"guest"`
- `RabbitMQ__Exchange`: `"fcg.catalog.events"` (saída de pedidos)
- `RabbitMQ__PaymentsExchange`: `"fcg.payments.events"` (entrada de pagamentos)
- `RabbitMQ__PaymentProcessedQueue`: `"catalog.payment-processed"`
- `RabbitMQ__PaymentProcessedRoutingKey`: `"payment.*"`
- `Jwt__Issuer`: `"FCG02.UsersAPI"`
- `Jwt__Audience`: `"FCG02.CloudGamesClients"`

### 3.2. `Secret` (Credenciais em Base64)
- `ConnectionStrings__DefaultConnection`: String de conexão do Azure Database (`fcg-02-catalogdb`) em Base64.
- `RabbitMQ__Password`: `guest` em Base64.
- `Jwt__SecretKey`: Chave simétrica HMAC-SHA256 em Base64.

---

## 4. Especificação do `Deployment`

### 4.1. Réplicas e Alta Disponibilidade
Mantém `replicas: 2`. Ambas as réplicas mantêm o worker consumidor `PaymentProcessedConsumerService` ativo, dividindo a carga de mensagens de forma equilibrada através do RabbitMQ.

### 4.2. Injeção de Variáveis via `envFrom`
Todas as variáveis do ConfigMap e do Secret são injetadas automaticamente no contêiner C# sem necessidade de mapeamentos manuais extensos.

### 4.3. Sondas de Integridade (Readiness e Liveness Probes)
- **`readinessProbe`:** Verifica `GET /health` na porta 8080 a cada 10s (`initialDelaySeconds: 5`). O Pod só recebe requisições de compra e catálogo se estiver pronto.
- **`livenessProbe`:** Verifica `GET /health` a cada 20s (`initialDelaySeconds: 15`). Reinicia o Pod caso ocorra deadlock no processo do .NET.

### 4.4. Limites e Requisições de Recursos (Resources)
- **`requests`:** CPU `100m` e Memória `128Mi`.
- **`limits`:** CPU `300m` e Memória `256Mi`.

---

## 5. Exposição de Rede com `Service` (ClusterIP)

- **Tipo:** `ClusterIP`
- **Nome DNS Interno:** `catalog-api-service`
- **Mapeamento:** Porta 80 externa do serviço direcionada para a porta 8080 interna do contêiner.

---

## 6. Comandos Essenciais de Operação (`kubectl`)

```powershell
# Validação prévia de sintaxe (dry-run)
kubectl apply -f k8s/ --dry-run=client

# Aplicar manifestos
kubectl apply -f k8s/

# Monitorar Pods
kubectl get pods -l app=catalog-api

# Logs em tempo real
kubectl logs -f -l app=catalog-api

# Teste local via túnel Port-Forward
kubectl port-forward svc/catalog-api-service 8082:80
```

---

## 7. Navegação

- ⬅️ **[Voltar para a Containerização Docker (`docker.md`)](docker.md)**
- 📑 **[Voltar para o Índice da Documentação (`index.md`)](index.md)**
