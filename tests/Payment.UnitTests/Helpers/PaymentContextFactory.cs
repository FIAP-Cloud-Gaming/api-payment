namespace Payment.UnitTests.Helpers;

internal static class PaymentContextFactory
{
    public static PaymentContext Create(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<PaymentContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var mediator = Substitute.For<IMediator>();
        return new PaymentContext(options, mediator);
    }
}
