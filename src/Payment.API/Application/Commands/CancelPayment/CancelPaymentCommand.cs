namespace Payment.API.Application.Commands.CancelPayment;

public record CancelPaymentCommand(
    string PaymentId,
    string Reason
) : ICommand<PaymentDto>;

public class CancelPaymentCommandHandler(IPaymentRepository paymentRepository)
    : ICommandHandler<CancelPaymentCommand, PaymentDto>
{
    public async Task<PaymentDto> Handle(CancelPaymentCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var payment = await paymentRepository.GetByIdAsync(PaymentId.From(command.PaymentId), cancellationToken)
            ?? throw new KeyNotFoundException($"Payment with ID {command.PaymentId} not found");

        payment.Cancel(command.Reason);

        paymentRepository.Update(payment);
        await paymentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return PaymentDto.FromPayment(payment);
    }
}
