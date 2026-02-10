using Payment.API.Application.Commands.CreatePayment;
using Payment.API.Application.Commands.ProcessPayment;
using Payment.API.Application.Commands.CancelPayment;
using Payment.API.Application.Commands.RefundPayment;
using Payment.API.Application.Queries.GetPayment;
using Payment.API.Application.Queries.GetPaymentsByUser;
using Payment.API.Application.Queries.GetPaymentsByStatus;

namespace Payment.API.Apis;

public static class PaymentApi
{
    public static RouteGroupBuilder MapPaymentsApi(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/payments").WithTags("Payments");

        api.MapPost("/", CreatePayment)
            .WithName("CreatePayment")
            .WithDescription("Create a new payment")
            .Produces<PaymentDto>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        api.MapGet("/{id}", GetPayment)
            .WithName("GetPayment")
            .WithDescription("Get a payment by its ID")
            .Produces<PaymentDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        api.MapPost("/{id}/process", ProcessPayment)
            .WithName("ProcessPayment")
            .WithDescription("Process a payment and mark it as succeeded")
            .Produces<PaymentDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        api.MapPost("/{id}/cancel", CancelPayment)
            .WithName("CancelPayment")
            .WithDescription("Cancel a payment")
            .Produces<PaymentDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        api.MapPost("/{id}/refund", RefundPayment)
            .WithName("RefundPayment")
            .WithDescription("Refund a succeeded payment")
            .Produces<PaymentDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        api.MapGet("/user/{userId}", GetPaymentsByUser)
            .WithName("GetPaymentsByUser")
            .WithDescription("Get all payments for a specific user")
            .Produces<IEnumerable<PaymentDto>>(StatusCodes.Status200OK);

        api.MapGet("/status/{status}", GetPaymentsByStatus)
            .WithName("GetPaymentsByStatus")
            .WithDescription("Get all payments by status (Pending, Processing, Succeeded, Failed, Cancelled, Refunded)")
            .Produces<IEnumerable<PaymentDto>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        return api;
    }

    private static async Task<IResult> CreatePayment(
        [FromBody] CreatePaymentRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = CreatePaymentCommand.FromRequest(request);
        var result = await mediator.Send(command, cancellationToken);
        return TypedResults.Created($"/api/payments/{result.Id}", result);
    }

    private static async Task<IResult> GetPayment(
        [FromRoute] string id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetPaymentQuery(id);
        var result = await mediator.Receive(query, cancellationToken);

        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound(new ProblemDetails
            {
                Title = "Not Found",
                Detail = $"Payment with ID {id} not found.",
                Status = StatusCodes.Status404NotFound
            });
    }

    private static async Task<IResult> ProcessPayment(
        [FromRoute] string id,
        [FromBody] ProcessPaymentRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = ProcessPaymentCommand.FromRequest(id, request);
        var result = await mediator.Send(command, cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> CancelPayment(
        [FromRoute] string id,
        [FromBody] CancelPaymentRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new CancelPaymentCommand(id, request.Reason);
        var result = await mediator.Send(command, cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> RefundPayment(
        [FromRoute] string id,
        [FromBody] RefundPaymentRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new RefundPaymentCommand(id, request.Reason);
        var result = await mediator.Send(command, cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> GetPaymentsByUser(
        [FromRoute] string userId,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetPaymentsByUserQuery(userId);
        var result = await mediator.Receive(query, cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<IResult> GetPaymentsByStatus(
        [FromRoute] PaymentStatus status,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetPaymentsByStatusQuery(status);
        var result = await mediator.Receive(query, cancellationToken);
        return TypedResults.Ok(result);
    }
}

public record CancelPaymentRequest
{
    public required string Reason { get; init; }
}

public record RefundPaymentRequest
{
    public required string Reason { get; init; }
}
