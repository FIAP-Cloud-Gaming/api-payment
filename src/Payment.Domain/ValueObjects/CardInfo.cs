namespace Payment.Domain.ValueObjects;

public record CardInfo
{
    public string CardHolderName { get; init; }
    public string MaskedCardNumber { get; init; }
    public string CardBrand { get; init; }
    public int ExpirationMonth { get; init; }
    public int ExpirationYear { get; init; }

    private CardInfo(
        string cardHolderName,
        string maskedCardNumber,
        string cardBrand,
        int expirationMonth,
        int expirationYear)
    {
        if (string.IsNullOrWhiteSpace(cardHolderName))
            throw new ArgumentException("Card holder name is required", nameof(cardHolderName));

        if (string.IsNullOrWhiteSpace(maskedCardNumber))
            throw new ArgumentException("Card number is required", nameof(maskedCardNumber));

        if (expirationMonth < 1 || expirationMonth > 12)
            throw new ArgumentException("Invalid expiration month", nameof(expirationMonth));

        var currentYear = DateTime.UtcNow.Year;
        if (expirationYear < currentYear || expirationYear > currentYear + 20)
            throw new ArgumentException("Invalid expiration year", nameof(expirationYear));

        CardHolderName = cardHolderName;
        MaskedCardNumber = maskedCardNumber;
        CardBrand = cardBrand ?? "Unknown";
        ExpirationMonth = expirationMonth;
        ExpirationYear = expirationYear;
    }

    public static CardInfo Create(
        string cardHolderName,
        string cardNumber,
        string cardBrand,
        int expirationMonth,
        int expirationYear)
    {
        var maskedNumber = MaskCardNumber(cardNumber);
        return new CardInfo(cardHolderName, maskedNumber, cardBrand, expirationMonth, expirationYear);
    }

    private static string MaskCardNumber(string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber))
            return "****";

        var cleaned = cardNumber.Replace(" ", "").Replace("-", "");

        if (cleaned.Length < 4)
            return "****";

        var lastFour = cleaned[^4..];
        return $"**** **** **** {lastFour}";
    }

    public bool IsExpired()
    {
        var now = DateTime.UtcNow;
        if (ExpirationYear < now.Year)
            return true;

        if (ExpirationYear == now.Year && ExpirationMonth < now.Month)
            return true;

        return false;
    }

    public override string ToString()
        => $"{CardBrand} {MaskedCardNumber} ({ExpirationMonth:D2}/{ExpirationYear})";
}
