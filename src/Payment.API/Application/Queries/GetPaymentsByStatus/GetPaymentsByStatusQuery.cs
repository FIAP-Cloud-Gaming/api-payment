namespace Payment.API.Application.Queries.GetPaymentsByStatus;

public record GetPaymentsByStatusQuery(PaymentStatus Status) : IQuery<IEnumerable<PaymentDto>>;

public class GetPaymentsByStatusQueryHandler(IPaymentRepository paymentRepository)
    : IQueryHandler<GetPaymentsByStatusQuery, IEnumerable<PaymentDto>>
{
    public async Task<IEnumerable<PaymentDto>> Handle(GetPaymentsByStatusQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var payments = await paymentRepository.GetByStatusAsync(query.Status, cancellationToken);

        return [.. payments.Select(PaymentDto.FromPayment)];
    }
}
