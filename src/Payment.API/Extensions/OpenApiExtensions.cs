namespace Payment.API.Extensions;

public static class OpenApiExtensions
{
    public static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer(static (document, context, cancellationToken) =>
            {
                document.Info = new()
                {
                    Title = "FCG Payment API",
                    Version = "v1",
                    Description = "Payment Service - Microserviço de Pagamentos",
                    Contact = new()
                    {
                        Name = "FCG Team",
                        Email = "dev@fcg.com"
                    }
                };

                document.Tags = new HashSet<OpenApiTag>
                {
                    new() { Name = "Payments", Description = "Endpoints de gerenciamento de pagamentos" }
                };

                return Task.CompletedTask;
            });

            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
        });

        return services;
    }

    public static IApplicationBuilder UseOpenApiDocumentation(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();

            app.MapScalarApiReference(options =>
            {
                options
                    .WithTitle("FCG Payment API")
                    .WithTheme(ScalarTheme.Purple)
                    .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
            });

            var logger = app.Services.GetRequiredService<ILogger<Program>>();

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("📚 API Documentation available at: https://localhost:{Port}/scalar/v1",
                        app.Configuration["ASPNETCORE_HTTPS_PORT"] ?? "5002");
            }

        }

        return app;
    }
}

internal sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider provider)
    : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var schemes = await provider.GetAllSchemesAsync();

        if (!schemes.Any(authScheme => authScheme.Name == JwtBearerDefaults.AuthenticationScheme))
            return;

        var securitySchemes = new Dictionary<string, IOpenApiSecurityScheme>
        {
            [JwtBearerDefaults.AuthenticationScheme] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme.ToLowerInvariant(),
                In = ParameterLocation.Header,
                BearerFormat = "Json Web Token"
            }
        };

        document.Components ??= new();
        document.Components.SecuritySchemes = securitySchemes;

        if (document.Paths is null)
            return;

        foreach (var path in document.Paths.Values)
        {
            if (path.Operations is null)
                continue;

            foreach (var operation in path.Operations)
            {
                if (operation.Value is null)
                    return;

                operation.Value.Security ??= [];
                operation.Value.Security.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference(JwtBearerDefaults.AuthenticationScheme, document)] = []
                });
            }
        }
    }
}
