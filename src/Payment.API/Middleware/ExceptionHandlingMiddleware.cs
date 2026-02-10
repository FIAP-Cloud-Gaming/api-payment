using Payment.Domain.Exceptions;

namespace Payment.API.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        if (_logger.IsEnabled(LogLevel.Error))
        {
            _logger.LogError(exception, "An error occurred: {ExceptionType} - {Message}",
                exception.GetType().Name,
                exception.Message);
        }

        var (statusCode, response) = exception switch
        {
            ValidationException validationEx => (
                StatusCodes.Status400BadRequest,
                CreateValidationProblem(validationEx, context.Request.Path)),

            PaymentException domainEx => (
                StatusCodes.Status400BadRequest,
                CreateDomainProblem(domainEx, context.Request.Path)),

            KeyNotFoundException => (
                StatusCodes.Status404NotFound,
                CreateNotFoundProblem(context.Request.Path)),

            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                CreateUnauthorizedProblem(context.Request.Path)),

            _ => (
                StatusCodes.Status500InternalServerError,
                CreateServerErrorProblem(context.Request.Path))
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(response);
    }

    private static ProblemDetails CreateValidationProblem(ValidationException ex, string? path) => new()
    {
        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        Title = "Validation Error",
        Status = StatusCodes.Status400BadRequest,
        Detail = "One or more validation errors occurred.",
        Instance = path,
        Extensions =
        {
            ["errors"] = ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray())
        }
    };

    private static ProblemDetails CreateDomainProblem(PaymentException ex, string? path) => new()
    {
        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        Title = "Domain Error",
        Status = StatusCodes.Status400BadRequest,
        Detail = ex.Message,
        Instance = path
    };

    private static ProblemDetails CreateNotFoundProblem(string? path) => new()
    {
        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        Title = "Not Found",
        Status = StatusCodes.Status404NotFound,
        Detail = "The requested resource was not found.",
        Instance = path
    };

    private static ProblemDetails CreateUnauthorizedProblem(string? path) => new()
    {
        Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
        Title = "Unauthorized",
        Status = StatusCodes.Status401Unauthorized,
        Detail = "Authentication required.",
        Instance = path
    };

    private static ProblemDetails CreateServerErrorProblem(string? path) => new()
    {
        Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
        Title = "Server Error",
        Status = StatusCodes.Status500InternalServerError,
        Detail = "An unexpected error occurred.",
        Instance = path
    };
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
        => app.UseMiddleware<ExceptionHandlingMiddleware>();
}
