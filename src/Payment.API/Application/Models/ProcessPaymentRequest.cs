namespace Payment.API.Application.Models;

public sealed record ProcessPaymentRequest
{
    public required string TransactionId { get; init; }
    public string? AuthorizationCode { get; init; }
}
