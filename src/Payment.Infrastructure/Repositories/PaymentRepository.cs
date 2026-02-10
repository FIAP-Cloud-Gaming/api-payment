namespace Payment.Infrastructure.Repositories;

public class PaymentRepository(PaymentContext context) : IPaymentRepository
{
    private readonly PaymentContext _context = context
        ?? throw new ArgumentNullException(nameof(context));

    public IUnitOfWork UnitOfWork => _context;

    public Domain.AggregatesModel.PaymentAggregate.Payment Add(Domain.AggregatesModel.PaymentAggregate.Payment payment)
    {
        return _context.Payments.Add(payment).Entity;
    }

    public void Update(Domain.AggregatesModel.PaymentAggregate.Payment payment)
    {
        _context.Entry(payment).State = EntityState.Modified;
    }

    public async Task<Domain.AggregatesModel.PaymentAggregate.Payment?> GetByIdAsync(
        PaymentId id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Domain.AggregatesModel.PaymentAggregate.Payment?> GetByOrderIdAsync(
        string orderId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.OrderId == orderId, cancellationToken);
    }

    public async Task<IEnumerable<Domain.AggregatesModel.PaymentAggregate.Payment>> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Include(p => p.Items)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Domain.AggregatesModel.PaymentAggregate.Payment>> GetByStatusAsync(
        PaymentStatus status,
        CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Include(p => p.Items)
            .Where(p => p.Status == status)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Domain.AggregatesModel.PaymentAggregate.Payment>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Include(p => p.Items)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Domain.AggregatesModel.PaymentAggregate.Payment>> GetPendingPaymentsOlderThanAsync(
        int minutes,
        CancellationToken cancellationToken = default)
    {
        var threshold = DateTime.UtcNow.AddMinutes(-minutes);

        return await _context.Payments
            .Include(p => p.Items)
            .Where(p => p.Status == PaymentStatus.Pending && p.CreatedAt < threshold)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
