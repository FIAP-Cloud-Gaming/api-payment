# Grafana Configuration - Payment Service

Configuracao de observabilidade para o servico de Payment usando Grafana, Loki e Jaeger.

## Estrutura

```
grafana/
├── README.md
└── provisioning/
    ├── datasources/
    │   └── datasources.yml      Configuracao Loki + Jaeger
    └── dashboards/
        ├── dashboards.yml        Provisionamento automatico
        └── payment-api-dashboard.json   Dashboard do Payment API
```

## Dashboard: Payment API - Payment Management Service

Dashboard customizado para monitoramento do servico de Payment.

### Paineis Incluidos

1. **Total Payments Created** - Contador total de pagamentos criados no periodo
2. **Total Succeeded** - Contador total de pagamentos bem-sucedidos
3. **Total Errors** - Contador total de erros (Error + Fatal)
4. **Log Levels Distribution** - Distribuicao de logs por nivel (Info, Warning, Error)
5. **Payment Activity Over Time** - Serie temporal de criacao vs sucesso de pagamentos
6. **Payment API Logs (Real-time)** - Logs em tempo real do container
7. **Logs Over Time by Level** - Serie temporal de logs agrupados por nivel
8. **Error Logs** - Logs filtrados apenas por erros
9. **Payment Processing Logs** - Logs de operacoes de processamento de pagamentos
10. **Refund & Cancel Logs** - Logs de cancelamento e reembolso

### Queries LogQL Utilizadas

#### Logs de Erro
```logql
{container="fcg-payment-api"} | json | level=~"Error|Fatal" | __error__=""
```

#### Logs de Processamento de Pagamentos
```logql
{container="fcg-payment-api"} |~ "(?i)(create|process|succeed|fail)" | json
```

#### Logs de Cancelamento e Reembolso
```logql
{container="fcg-payment-api"} |~ "(?i)(cancel|refund)" | json
```

#### Contadores de Atividade
```logql
# Pagamentos Criados
count_over_time({container="fcg-payment-api"} |~ "(?i)payment.*created" | json | __error__="" [$__range])

# Pagamentos Bem-sucedidos
count_over_time({container="fcg-payment-api"} |~ "(?i)succeeded" | json | __error__="" [$__range])
```

## Como Usar

### 1. Iniciar os Servicos
```bash
docker compose up -d
```

### 2. Acessar Grafana
- URL: http://localhost:3000
- Usuario: `admin`
- Senha: `admin`

### 3. Dashboard Pre-configurado
O dashboard e provisionado automaticamente em:
- **Pasta**: FCG
- **Nome**: Payment API - Payment Management Service

### 4. Explorar Logs
Use o Explore para queries ad-hoc:
```logql
{container="fcg-payment-api"} | json
```

### 5. Visualizar Traces
- Acesse o painel de traces
- Selecione datasource: Jaeger
- Traces automaticamente correlacionados com logs

## Datasources Configurados

### Loki (Default)
- URL: http://loki:3100
- Tipo: Loki
- Uso: Logs estruturados da aplicacao

### Jaeger
- URL: http://jaeger:16686
- Tipo: Jaeger
- Uso: Distributed tracing
- Integracao: Correlacao automatica com logs Loki

## Personalizacao

Os arquivos de provisionamento podem ser editados:
- `datasources.yml` - Adicionar novos datasources
- `dashboards.yml` - Configurar pastas e providers
- `payment-api-dashboard.json` - Customizar paineis e queries

Mudancas sao aplicadas automaticamente (updateIntervalSeconds: 10).

## Troubleshooting

### Dashboard nao aparece
- Verifique se o Grafana esta rodando: `docker compose ps`
- Verifique logs: `docker compose logs grafana`
- Verifique montagem de volumes no docker-compose.yml

### Logs nao aparecem
- Verifique se o Loki esta rodando: `docker compose ps loki`
- Verifique se os logs estao sendo enviados: `docker compose logs payment-api`
- Teste query no Explore: `{container="fcg-payment-api"}`

### Traces nao aparecem
- Verifique se o Jaeger esta rodando: `docker compose ps jaeger`
- Acesse Jaeger UI diretamente: http://localhost:16686
- Verifique se OpenTelemetry esta configurado no ServiceDefaults
