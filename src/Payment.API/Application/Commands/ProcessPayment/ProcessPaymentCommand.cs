namespace Payment.API.Application.Commands.ProcessPayment;

public record ProcessPaymentCommand(
    string PaymentId,
    string TransactionId,
    string? AuthorizationCode
) : ICommand<PaymentDto>
{
    public static ProcessPaymentCommand FromRequest(string paymentId, ProcessPaymentRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return new ProcessPaymentCommand(
            paymentId,
            request.TransactionId,
            request.AuthorizationCode
        );
    }
}

public class ProcessPaymentCommandHandler(IPaymentRepository paymentRepository)
    : ICommandHandler<ProcessPaymentCommand, PaymentDto>
{
    public async Task<PaymentDto> Handle(ProcessPaymentCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var payment = await paymentRepository.GetByIdAsync(PaymentId.From(command.PaymentId), cancellationToken)
            ?? throw new KeyNotFoundException($"Payment with ID {command.PaymentId} not found");

        payment.StartProcessing();
        payment.MarkAsSucceeded(command.TransactionId, command.AuthorizationCode);

        paymentRepository.Update(payment);
        await paymentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return PaymentDto.FromPayment(payment);
    }
}
