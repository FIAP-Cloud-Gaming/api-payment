namespace Payment.Domain.Events;

public record PaymentCancelledDomainEvent(PaymentAggregate Payment, string Reason) : INotification;
