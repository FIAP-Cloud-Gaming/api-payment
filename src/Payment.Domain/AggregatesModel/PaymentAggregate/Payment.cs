namespace Payment.Domain.AggregatesModel.PaymentAggregate;

public class Payment : EntityWithNotification, IAggregateRoot
{
    private readonly List<PaymentItem> _items;

    public new PaymentId Id { get; private set; }
    public string OrderId { get; private set; }
    public string UserId { get; private set; }
    public Money TotalAmount { get; private set; }
    public PaymentStatus Status { get; private set; }
    public PaymentMethod Method { get; private set; }
    public PaymentDetails Details { get; private set; }
    public CardInfo? CardInfo { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public IReadOnlyCollection<PaymentItem> Items => _items.AsReadOnly();

    private Payment()
    {
        _items = [];
        Id = null!;
        OrderId = null!;
        UserId = null!;
        TotalAmount = null!;
        Details = PaymentDetails.Empty();
    }

    public static Payment Create(
        string orderId,
        string userId,
        PaymentMethod method,
        IEnumerable<PaymentItem> items,
        CardInfo? cardInfo = null)
    {
        if (string.IsNullOrWhiteSpace(orderId))
            throw new PaymentException("Order ID is required");

        if (string.IsNullOrWhiteSpace(userId))
            throw new PaymentException("User ID is required");

        if (items == null || !items.Any())
            throw new PaymentException("Payment must have at least one item");

        var payment = new Payment
        {
            Id = PaymentId.NewId(),
            OrderId = orderId,
            UserId = userId,
            Method = method,
            Status = PaymentStatus.Pending,
            Details = PaymentDetails.Empty(),
            CardInfo = cardInfo,
            CreatedAt = DateTime.UtcNow,
            TotalAmount = Money.Zero()
        };

        foreach (var item in items)
        {
            payment.AddItem(item);
        }

        if (method == PaymentMethod.CreditCard || method == PaymentMethod.DebitCard)
        {
            if (cardInfo == null)
                throw new PaymentException($"{method} requires card information");

            if (cardInfo.IsExpired())
                throw new PaymentException("Card is expired");
        }

        payment.AddNotification(new PaymentStartedDomainEvent(payment));

        return payment;
    }

    private void AddItem(PaymentItem item)
    {
        _items.Add(item);
        RecalculateTotal();
    }

    private void RecalculateTotal()
    {
        if (_items.Count != 0)
        {
            var currency = _items.First().Amount.Currency;
            var total = _items.Sum(i => i.Amount.Value);
            TotalAmount = Money.FromDecimal(total, currency);
        }
        else
        {
            TotalAmount = Money.Zero();
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void StartProcessing()
    {
        if (Status != PaymentStatus.Pending)
            throw new PaymentException($"Cannot start processing payment in {Status} status");

        Status = PaymentStatus.Processing;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsSucceeded(string transactionId, string? authorizationCode = null)
    {
        if (Status != PaymentStatus.Processing && Status != PaymentStatus.Pending)
            throw new PaymentException($"Cannot mark payment as succeeded from {Status} status");

        if (string.IsNullOrWhiteSpace(transactionId))
            throw new PaymentException("Transaction ID is required");

        Status = PaymentStatus.Succeeded;
        Details = Details
            .WithTransactionId(transactionId)
            .WithAuthorizationCode(authorizationCode ?? string.Empty);
        ProcessedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddNotification(new PaymentSucceededDomainEvent(this));
    }

    public void MarkAsFailed(string failureReason, string? processorResponse = null)
    {
        if (Status != PaymentStatus.Processing && Status != PaymentStatus.Pending)
            throw new PaymentException($"Cannot mark payment as failed from {Status} status");

        if (string.IsNullOrWhiteSpace(failureReason))
            throw new PaymentException("Failure reason is required");

        Status = PaymentStatus.Failed;
        Details = Details
            .WithFailureReason(failureReason)
            .WithProcessorResponse(processorResponse ?? string.Empty);
        ProcessedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddNotification(new PaymentFailedDomainEvent(this, failureReason));
    }

    public void Cancel(string reason)
    {
        if (Status == PaymentStatus.Succeeded)
            throw new PaymentException("Cannot cancel a succeeded payment. Use refund instead.");

        if (Status == PaymentStatus.Cancelled)
            throw new PaymentException("Payment is already cancelled");

        if (Status == PaymentStatus.Refunded)
            throw new PaymentException("Cannot cancel a refunded payment");

        Status = PaymentStatus.Cancelled;
        Details = Details.WithFailureReason($"Cancelled: {reason}");
        ProcessedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddNotification(new PaymentCancelledDomainEvent(this, reason));
    }

    public void Refund(string reason)
    {
        if (Status != PaymentStatus.Succeeded)
            throw new PaymentException("Only succeeded payments can be refunded");

        if (string.IsNullOrWhiteSpace(reason))
            throw new PaymentException("Refund reason is required");

        Status = PaymentStatus.Refunded;
        UpdatedAt = DateTime.UtcNow;

        AddNotification(new PaymentRefundedDomainEvent(this, reason));
    }

    public bool CanBeProcessed()
        => Status == PaymentStatus.Pending;

    public bool CanBeCancelled()
        => Status == PaymentStatus.Pending || Status == PaymentStatus.Processing;

    public bool CanBeRefunded()
        => Status == PaymentStatus.Succeeded;

    public bool IsInFinalState()
        => Status == PaymentStatus.Succeeded ||
           Status == PaymentStatus.Failed ||
           Status == PaymentStatus.Cancelled ||
           Status == PaymentStatus.Refunded;
}
