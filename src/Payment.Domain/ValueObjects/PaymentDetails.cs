namespace Payment.Domain.ValueObjects;

public record PaymentDetails
{
    public string? TransactionId { get; init; }
    public string? AuthorizationCode { get; init; }
    public string? FailureReason { get; init; }
    public string? ProcessorResponse { get; init; }
    public DateTime? ProcessedAt { get; init; }

    private PaymentDetails() { }

    public static PaymentDetails Empty() => new();

    public static PaymentDetails Create(
        string? transactionId = null,
        string? authorizationCode = null,
        string? processorResponse = null)
        => new()
        {
            TransactionId = transactionId,
            AuthorizationCode = authorizationCode,
            ProcessorResponse = processorResponse,
            ProcessedAt = transactionId != null ? DateTime.UtcNow : null
        };

    public PaymentDetails WithTransactionId(string transactionId)
        => this with
        {
            TransactionId = transactionId,
            ProcessedAt = DateTime.UtcNow
        };

    public PaymentDetails WithAuthorizationCode(string authorizationCode)
        => this with { AuthorizationCode = authorizationCode };

    public PaymentDetails WithFailureReason(string failureReason)
        => this with
        {
            FailureReason = failureReason,
            ProcessedAt = DateTime.UtcNow
        };

    public PaymentDetails WithProcessorResponse(string processorResponse)
        => this with { ProcessorResponse = processorResponse };

    public bool IsProcessed => !string.IsNullOrEmpty(TransactionId);
    public bool HasFailed => !string.IsNullOrEmpty(FailureReason);
}
