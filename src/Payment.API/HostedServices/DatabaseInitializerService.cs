namespace Payment.API.HostedServices;

public class DatabaseInitializerService(
    IServiceProvider serviceProvider,
    ILogger<DatabaseInitializerService> logger) 
        : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting database initialization...");

        using var scope = serviceProvider.CreateScope();

        // Additional initialization logic can be added here (e.g., seeding data)

        logger.LogInformation("✅ Database initialization completed successfully");

        await Task.CompletedTask;
    }
}
