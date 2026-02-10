namespace Payment.Domain.AggregatesModel.PaymentAggregate;

public interface IPaymentRepository : IRepository<Payment>
{
    Payment Add(Payment payment);
    void Update(Payment payment);
    Task<Payment?> GetByIdAsync(PaymentId id, CancellationToken cancellationToken = default);
    Task<Payment?> GetByOrderIdAsync(string orderId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetPendingPaymentsOlderThanAsync(int minutes, CancellationToken cancellationToken = default);
}
