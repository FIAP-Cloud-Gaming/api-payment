namespace Payment.Domain.Events;

public record PaymentFailedDomainEvent(PaymentAggregate Payment, string FailureReason) : INotification;
