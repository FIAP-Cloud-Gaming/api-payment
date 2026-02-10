using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Payment.UnitTests.Application.Middleware;

public sealed class ExceptionHandlingMiddlewareTests
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger = Substitute.For<ILogger<ExceptionHandlingMiddleware>>();
    private readonly ExceptionHandlingMiddleware _middleware;

    public ExceptionHandlingMiddlewareTests()
    {
        RequestDelegate next = _ => Task.CompletedTask;
        _middleware = new ExceptionHandlingMiddleware(next, _logger);
    }

    [Fact]
    public async Task InvokeAsync_NoException_ShouldCallNext()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        var middleware = new ExceptionHandlingMiddleware(next, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task InvokeAsync_ValidationException_ShouldReturn400()
    {
        // Arrange
        var context = CreateHttpContext();
        var validationFailures = new[]
        {
            new ValidationFailure("Property1", "Error message 1"),
            new ValidationFailure("Property2", "Error message 2")
        };
        var exception = new ValidationException(validationFailures);

        RequestDelegate next = _ => throw exception;
        var middleware = new ExceptionHandlingMiddleware(next, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        context.Response.ContentType.Should().StartWith("application/json");

        var responseBody = await GetResponseBody(context);
        responseBody.Should().Contain("Validation Error");
    }

    [Fact]
    public async Task InvokeAsync_PaymentException_ShouldReturn400()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new PaymentException("Payment cannot be processed");

        RequestDelegate next = _ => throw exception;
        var middleware = new ExceptionHandlingMiddleware(next, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        context.Response.ContentType.Should().StartWith("application/json");

        var responseBody = await GetResponseBody(context);
        responseBody.Should().Contain("Domain Error");
        responseBody.Should().Contain("Payment cannot be processed");
    }

    [Fact]
    public async Task InvokeAsync_KeyNotFoundException_ShouldReturn404()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new KeyNotFoundException("Payment not found");

        RequestDelegate next = _ => throw exception;
        var middleware = new ExceptionHandlingMiddleware(next, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        context.Response.ContentType.Should().StartWith("application/json");

        var responseBody = await GetResponseBody(context);
        responseBody.Should().Contain("Not Found");
    }

    [Fact]
    public async Task InvokeAsync_UnauthorizedAccessException_ShouldReturn401()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new UnauthorizedAccessException("Unauthorized");

        RequestDelegate next = _ => throw exception;
        var middleware = new ExceptionHandlingMiddleware(next, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        context.Response.ContentType.Should().StartWith("application/json");

        var responseBody = await GetResponseBody(context);
        responseBody.Should().Contain("Unauthorized");
    }

    [Fact]
    public async Task InvokeAsync_UnhandledException_ShouldReturn500()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new InvalidOperationException("Something went wrong");

        RequestDelegate next = _ => throw exception;
        var middleware = new ExceptionHandlingMiddleware(next, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        context.Response.ContentType.Should().StartWith("application/json");

        var responseBody = await GetResponseBody(context);
        responseBody.Should().Contain("Server Error");
    }

    [Fact]
    public async Task InvokeAsync_ShouldWriteProblemDetailsJson()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new KeyNotFoundException("Resource not found");

        RequestDelegate next = _ => throw exception;
        var middleware = new ExceptionHandlingMiddleware(next, _logger);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var responseBody = await GetResponseBody(context);
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody);

        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(StatusCodes.Status404NotFound);
        problemDetails.Title.Should().NotBeNullOrEmpty();
        problemDetails.Type.Should().NotBeNullOrEmpty();
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<string> GetResponseBody(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }
}
