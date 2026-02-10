using Microsoft.EntityFrameworkCore.Design;

namespace Payment.Infrastructure;

public class PaymentContextDesignTimeFactory : IDesignTimeDbContextFactory<PaymentContext>
{
    public PaymentContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PaymentContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=PaymentDB;Username=postgres;Password=postgres",
            npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(PaymentContext).Assembly.FullName);
            });

        return new PaymentContext(optionsBuilder.Options, null!);
    }
}
