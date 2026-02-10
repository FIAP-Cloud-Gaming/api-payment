namespace Payment.API.Application.Commands.RefundPayment;

public record RefundPaymentCommand(
    string PaymentId,
    string Reason
) : ICommand<PaymentDto>;

public class RefundPaymentCommandHandler(IPaymentRepository paymentRepository)
    : ICommandHandler<RefundPaymentCommand, PaymentDto>
{
    public async Task<PaymentDto> Handle(RefundPaymentCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var payment = await paymentRepository.GetByIdAsync(PaymentId.From(command.PaymentId), cancellationToken)
            ?? throw new KeyNotFoundException($"Payment with ID {command.PaymentId} not found");

        payment.Refund(command.Reason);

        paymentRepository.Update(payment);
        await paymentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return PaymentDto.FromPayment(payment);
    }
}
