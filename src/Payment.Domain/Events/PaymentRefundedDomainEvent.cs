namespace Payment.Domain.Events;

public record PaymentRefundedDomainEvent(PaymentAggregate Payment, string Reason) : INotification;
