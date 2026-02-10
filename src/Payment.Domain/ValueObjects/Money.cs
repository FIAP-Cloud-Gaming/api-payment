namespace Payment.Domain.ValueObjects;

public record Money
{
    public decimal Value { get; init; }
    public string Currency { get; init; }

    private Money(decimal value, string currency)
    {
        if (value < 0)
            throw new ArgumentException("Money value cannot be negative", nameof(value));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be empty", nameof(currency));

        Value = Math.Round(value, 2);
        Currency = currency.ToUpperInvariant();
    }

    public static Money FromDecimal(decimal value, string currency = "BRL")
        => new(value, currency);

    public static Money Zero(string currency = "BRL")
        => new(0, currency);

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot add {other.Currency} to {Currency}");

        return new Money(Value + other.Value, Currency);
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot subtract {other.Currency} from {Currency}");

        if (Value < other.Value)
            throw new InvalidOperationException("Cannot subtract to negative value");

        return new Money(Value - other.Value, Currency);
    }

    public Money Multiply(decimal multiplier)
    {
        if (multiplier < 0)
            throw new ArgumentException("Multiplier cannot be negative", nameof(multiplier));

        return new Money(Value * multiplier, Currency);
    }

    public bool IsGreaterThan(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot compare {other.Currency} with {Currency}");

        return Value > other.Value;
    }

    public bool IsLessThan(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot compare {other.Currency} with {Currency}");

        return Value < other.Value;
    }

    public override string ToString() => $"{Currency} {Value:N2}";
}
