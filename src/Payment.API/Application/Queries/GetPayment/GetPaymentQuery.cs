namespace Payment.API.Application.Queries.GetPayment;

public record GetPaymentQuery(string PaymentId) : IQuery<PaymentDto?>;

public class GetPaymentQueryHandler(IPaymentRepository paymentRepository)
    : IQueryHandler<GetPaymentQuery, PaymentDto?>
{
    public async Task<PaymentDto?> Handle(GetPaymentQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var payment = await paymentRepository.GetByIdAsync(
            PaymentId.From(query.PaymentId),
            cancellationToken);

        return payment is null ? null : PaymentDto.FromPayment(payment);
    }
}
