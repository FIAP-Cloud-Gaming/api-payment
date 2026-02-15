using EventBus.Extensions;
using EventBusRabbitMQ;
using Payment.API.Application.IntegrationEvents;
using Payment.API.Application.IntegrationEvents.EventHandling;

namespace Payment.API.Extensions;

internal static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediator(assembly);

        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddScoped(typeof(IPipelineBehavior<>), typeof(TransactionBehavior<>));
        services.AddScoped(typeof(IPipelineBehavior<>), typeof(LoggingBehavior<>));
        services.AddScoped(typeof(IPipelineBehavior<>), typeof(ValidationBehavior<>));

        services.AddScoped<IPaymentIntegrationEventService, PaymentIntegrationEventService>();

        return services;
    }

    public static IServiceCollection AddCustomHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PaymentDB")
            ?? throw new InvalidOperationException("PaymentDB connection string not found");

        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"])
            .AddNpgSql(
                connectionString,
                name: "payment-db",
                tags: ["db", "postgres", "ready"]);

        return services;
    }

    public static IHostApplicationBuilder ConfigureEventBus(this IHostApplicationBuilder builder)
    {
        builder.AddRabbitMqEventBus(builder.Configuration.GetConnectionString("RabbitMQ")!, "payment-service");

        return builder;
    }

    public static IHostApplicationBuilder ConfigurePaymentSerilog(this IHostApplicationBuilder builder)
    {
        var elasticsearchUri = builder.Configuration["Elasticsearch:Uri"]
            ?? throw new InvalidOperationException("Elasticsearch:Uri not found in configuration");

        builder.Services.AddSerilog(loggerConfiguration =>
        {
            loggerConfiguration
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .Enrich.WithThreadId()
                .Enrich.WithProperty("Application", "Payment.API")
                .WriteTo
                    .Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} <s:{SourceContext}>{NewLine}{Exception}")
                .WriteTo
                .Elasticsearch(
                    [new Uri(elasticsearchUri)],
                    opts =>
                    {
                        opts.DataStream = new Elastic.Ingest.Elasticsearch.DataStreams.DataStreamName("logs", "payment-api");
                        opts.BootstrapMethod = Elastic.Ingest.Elasticsearch.BootstrapMethod.Failure;
                    },
                    transport =>
                    {
                        transport.Authentication(new Elastic.Transport.BasicAuthentication(
                            builder.Configuration["Elasticsearch:Username"]!,
                            builder.Configuration["Elasticsearch:Password"]!));
                    });
        });

        return builder;
    }
}
