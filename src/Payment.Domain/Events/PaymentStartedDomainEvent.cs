namespace Payment.Domain.Events;

public record PaymentStartedDomainEvent(PaymentAggregate Payment) : INotification;
