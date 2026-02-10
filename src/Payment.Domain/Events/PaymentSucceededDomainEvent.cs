namespace Payment.Domain.Events;

public record PaymentSucceededDomainEvent(PaymentAggregate Payment) : INotification;
