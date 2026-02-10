namespace Payment.API.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        logger.LogInformation("[START] {RequestName}", requestName);

        try
        {
            var response = await next(cancellationToken);

            logger.LogInformation("[END] {RequestName} completed successfully", requestName);

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[ERROR] {RequestName} failed", requestName);
            throw;
        }
    }
}

public class LoggingBehavior<TRequest>(ILogger<LoggingBehavior<TRequest>> logger)
    : IPipelineBehavior<TRequest>
    where TRequest : notnull
{
    public async Task Handle(
        TRequest request,
        RequestHandlerDelegate next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        logger.LogInformation("[START] {RequestName}", requestName);

        try
        {
            await next(cancellationToken);

            logger.LogInformation("[END] {RequestName} completed successfully", requestName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[ERROR] {RequestName} failed", requestName);
            throw;
        }
    }
}
