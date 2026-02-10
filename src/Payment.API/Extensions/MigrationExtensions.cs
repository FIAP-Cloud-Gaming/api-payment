namespace Payment.API.Extensions;

public static class MigrationExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();

        using PaymentContext dbContext =
            scope.ServiceProvider.GetRequiredService<PaymentContext>();

        dbContext.Database.Migrate();
    }
}
