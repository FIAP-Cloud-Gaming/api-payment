namespace Payment.API.Application.Queries.GetPaymentsByUser;

public record GetPaymentsByUserQuery(string UserId) : IQuery<IEnumerable<PaymentDto>>;

public class GetPaymentsByUserQueryHandler(IPaymentRepository paymentRepository)
    : IQueryHandler<GetPaymentsByUserQuery, IEnumerable<PaymentDto>>
{
    public async Task<IEnumerable<PaymentDto>> Handle(GetPaymentsByUserQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var payments = await paymentRepository.GetByUserIdAsync(query.UserId, cancellationToken);

        return [.. payments.Select(PaymentDto.FromPayment)];
    }
}
