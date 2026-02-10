namespace Payment.Domain.AggregatesModel.PaymentAggregate;

public enum PaymentStatus
{
    Pending = 1,
    Processing = 2,
    Succeeded = 3,
    Failed = 4,
    Cancelled = 5,
    Refunded = 6
}
