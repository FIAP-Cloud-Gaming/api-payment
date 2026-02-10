# FIAP Cloud Gaming - Payment Service

Microsservico de pagamentos da FIAP Cloud Gaming. Gerencia criacao, processamento, cancelamento e reembolso de pagamentos na plataforma.

## Arquitetura

Clean Architecture com Domain-Driven Design (DDD) e CQRS:

```
src/
├── Payment.Domain/           Entidades, Value Objects, Eventos de Dominio
├── Payment.Infrastructure/   EF Core, PostgreSQL, Repositorios
└── Payment.API/              Endpoints, Behaviors, Validacoes, Middleware
```

## Funcionalidades

### Pagamentos
- Criacao de pagamentos com multiplos metodos (Cartao Credito/Debito, Pix, Boleto, Wallet)
- Processamento de pagamentos com codigo de autorizacao
- Cancelamento de pagamentos pendentes
- Reembolso de pagamentos processados
- Consulta por usuario e status

### Metodos de Pagamento
- CreditCard, DebitCard, Pix, BankSlip, Wallet
- Validacao e mascaramento de dados de cartao
- Verificacao de expiracao

### Eventos de Dominio
- PaymentStarted, PaymentSucceeded, PaymentFailed
- PaymentCancelled, PaymentRefunded

## Endpoints

Todos os endpoints requerem autenticacao JWT.

### Payments

| Metodo | Rota | Descricao | Auth |
|--------|------|-----------|------|
| POST | `/api/payments` | Criar pagamento | Sim |
| GET | `/api/payments/{id}` | Buscar por ID | Sim |
| POST | `/api/payments/{id}/process` | Processar pagamento | Sim |
| POST | `/api/payments/{id}/cancel` | Cancelar pagamento | Sim |
| POST | `/api/payments/{id}/refund` | Reembolsar pagamento | Sim |
| GET | `/api/payments/user/{userId}` | Buscar por usuario | Sim |
| GET | `/api/payments/status/{status}` | Buscar por status | Sim |

## Stack

| Componente | Tecnologia |
|------------|------------|
| Framework | .NET 10.0 |
| CQRS | FiapCloudGaming.Mediator |
| DDD | FiapCloudGaming.SeedWork |
| Observabilidade | FiapCloudGaming.ServiceDefaults (OpenTelemetry + Serilog) |
| Banco relacional | PostgreSQL 18 + EF Core |
| Validacao | FluentValidation |
| Logging | Serilog + Elasticsearch + Grafana Loki |
| Tracing | Jaeger (OTLP) |
| Dashboards | Grafana |
| Documentacao API | Scalar (OpenAPI) |

## Executar localmente

### Pre-requisitos
- .NET 10.0 SDK
- Docker Desktop

### Docker Compose
```bash
docker compose up -d
```

### Servicos disponiveis

| Servico | URL |
|---------|-----|
| Payment API | http://localhost:5003 |
| Grafana | http://localhost:3000 (admin/admin) |
| Jaeger UI | http://localhost:16686 |
| PostgreSQL | localhost:5432 |
| Elasticsearch | http://localhost:9200 |

### Executar sem Docker
```bash
dotnet run --project src/Payment.API
```

## Observabilidade

### Logs
- Logs estruturados com Serilog
- Centralizacao via Elasticsearch e Grafana Loki
- Formato JSON com contexto completo

### Traces
- Traces distribuidos com OpenTelemetry
- Exportacao para Jaeger via OTLP
- Correlacao automatica entre requisicoes

### Health Checks
- `/health` - Health check geral do servico
- `/alive` - Liveness probe para Kubernetes

### Dashboards
- Dashboard Grafana pre-configurado
- Metricas de operacoes de pagamento
- Rastreamento de erros

## Dependencias (NuGet)

Pacotes compartilhados da solucao:
- `FiapCloudGaming.Mediator` - Implementacao CQRS com MediatR
- `FiapCloudGaming.SeedWork` - Building blocks DDD
- `FiapCloudGaming.ServiceDefaults` - Configuracoes de observabilidade

## Licenca

MIT
