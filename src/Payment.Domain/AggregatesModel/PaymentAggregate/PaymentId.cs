namespace Payment.Domain.AggregatesModel.PaymentAggregate;

public record PaymentId
{
    public Guid Value { get; init; }

    private PaymentId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PaymentId cannot be empty", nameof(value));

        Value = value;
    }

    public static PaymentId NewId() => new(Guid.NewGuid());

    public static PaymentId From(Guid value) => new(value);

    public static PaymentId From(string value)
    {
        if (!Guid.TryParse(value, out var guid))
            throw new ArgumentException($"Invalid PaymentId format: {value}", nameof(value));

        return new PaymentId(guid);
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(PaymentId id) => id.Value;
}
