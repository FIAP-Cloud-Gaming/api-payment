using Payment.API.Application.IntegrationEvents;

namespace Payment.API.Application.Behaviors;

internal sealed class TransactionBehavior<TCommand, TResponse>(
    PaymentContext context,
    IPaymentIntegrationEventService integrationEventService,
    ILogger<TransactionBehavior<TCommand, TResponse>> logger)
    : IPipelineBehavior<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
{
    public async Task<TResponse> Handle(
        TCommand command,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var response = default(TResponse);
        var typeName = command.GetType().Name;

        try
        {
            if (context.HasActiveTransaction)
            {
                logger.LogDebug("Transaction already active for {CommandName}, skipping transaction behavior", typeName);
                return await next(ct);
            }

            var strategy = context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                Guid transactionId;

                await using var transaction = await context.BeginTransactionAsync();

                if (transaction is null)
                {
                    response = await next(ct);
                    return;
                }

                using (logger.BeginScope(new List<KeyValuePair<string, object>> { new("TransactionContext", transaction.TransactionId) }))
                {
                    logger.LogInformation("Begin transaction {TransactionId} for {CommandName} ({@Command})",
                        transaction.TransactionId, typeName, command);

                    response = await next(ct);

                    logger.LogInformation("Commit transaction {TransactionId} for {CommandName}",
                        transaction.TransactionId, typeName);

                    await context.CommitTransactionAsync(transaction);

                    transactionId = transaction.TransactionId;
                }

                await integrationEventService.PublishEventsThroughEventBusAsync(transactionId);
            });

            return response!;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling transaction for {CommandName} ({@Command})", typeName, command);

            throw;
        }
    }
}

internal sealed class TransactionBehavior<TCommand>(
    PaymentContext context,
    IPaymentIntegrationEventService integrationEventService,
    ILogger<TransactionBehavior<TCommand>> logger)
    : IPipelineBehavior<TCommand>
        where TCommand : ICommand
{
    public async Task Handle(
        TCommand command,
        RequestHandlerDelegate next,
        CancellationToken ct)
    {
        var typeName = command.GetType().Name;

        try
        {
            if (context.HasActiveTransaction)
            {
                logger.LogDebug("Transaction already active for {CommandName}, skipping transaction behavior", typeName);
                await next(ct);
                return;
            }

            var strategy = context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                Guid transactionId;

                await using var transaction = await context.BeginTransactionAsync();

                if (transaction is null)
                {
                    await next(ct);
                    return;
                }

                using (logger.BeginScope(new List<KeyValuePair<string, object>> { new("TransactionContext", transaction.TransactionId) }))
                {
                    logger.LogInformation("Begin transaction {TransactionId} for {CommandName} ({@Command})",
                        transaction.TransactionId, typeName, command);

                    await next(ct);

                    logger.LogInformation("Commit transaction {TransactionId} for {CommandName}",
                        transaction.TransactionId, typeName);

                    await context.CommitTransactionAsync(transaction);

                    transactionId = transaction.TransactionId;
                }

                await integrationEventService.PublishEventsThroughEventBusAsync(transactionId);
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling transaction for {CommandName} ({@Command})", typeName, command);

            throw;
        }
    }
}
