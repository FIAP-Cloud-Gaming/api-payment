using Core.Defaults;
using Payment.API.Apis;
using Payment.API.Extensions;
using Payment.API.HostedServices;
using Payment.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigurePaymentSerilog();
builder.ConfigureOpenTelemetry();
builder.ConfigureEventBus();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddApplication();

builder.Services.AddCustomHealthChecks(builder.Configuration);

builder.Services.AddOpenApiDocumentation();

builder.Services.AddHostedService<DatabaseInitializerService>();

var app = builder.Build();

app.UseExceptionHandling();

app.ApplyMigrations();

app.MapDefaultEndpoints();

app.UseOpenApiDocumentation();

app.UseAuthentication();
app.UseAuthorization();

app.MapPaymentsApi();

app.Run();
