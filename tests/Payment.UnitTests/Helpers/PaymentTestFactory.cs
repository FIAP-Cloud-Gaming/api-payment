using DomainPayment = Payment.Domain.AggregatesModel.PaymentAggregate.Payment;

namespace Payment.UnitTests.Helpers;

internal static class PaymentTestFactory
{
    public static DomainPayment CreateValidPayment(
        string orderId = "ORDER-001",
        string userId = "user-123",
        PaymentMethod method = PaymentMethod.Pix,
        decimal amount = 199.99m,
        string currency = "BRL")
    {
        var items = new List<PaymentItem>
        {
            new("product-1", "Test Product", 1, Money.FromDecimal(amount, currency))
        };

        var payment = DomainPayment.Create(orderId, userId, method, items);
        payment.ClearNotification();
        return payment;
    }

    public static DomainPayment CreateValidPaymentWithCard(
        string orderId = "ORDER-001",
        string userId = "user-123",
        PaymentMethod method = PaymentMethod.CreditCard,
        decimal amount = 199.99m)
    {
        var items = new List<PaymentItem>
        {
            new("product-1", "Test Product", 1, Money.FromDecimal(amount, "BRL"))
        };

        var cardInfo = CardInfo.Create("John Doe", "4111111111111111", "Visa", 12, DateTime.UtcNow.Year + 2);

        var payment = DomainPayment.Create(orderId, userId, method, items, cardInfo);
        payment.ClearNotification();
        return payment;
    }

    public static DomainPayment CreateSucceededPayment(
        string orderId = "ORDER-001",
        string userId = "user-123",
        decimal amount = 199.99m)
    {
        var payment = CreateValidPayment(orderId, userId, amount: amount);
        payment.StartProcessing();
        payment.MarkAsSucceeded("TXN-001", "AUTH-001");
        payment.ClearNotification();
        return payment;
    }

    public static DomainPayment CreateProcessingPayment(
        string orderId = "ORDER-001",
        string userId = "user-123")
    {
        var payment = CreateValidPayment(orderId, userId);
        payment.StartProcessing();
        payment.ClearNotification();
        return payment;
    }

    public static DomainPayment CreateFailedPayment()
    {
        var payment = CreateProcessingPayment();
        payment.MarkAsFailed("Insufficient funds");
        payment.ClearNotification();
        return payment;
    }

    public static DomainPayment CreateCancelledPayment()
    {
        var payment = CreateValidPayment();
        payment.Cancel("User cancelled");
        payment.ClearNotification();
        return payment;
    }

    public static DomainPayment CreateRefundedPayment()
    {
        var payment = CreateSucceededPayment();
        payment.Refund("Customer requested");
        payment.ClearNotification();
        return payment;
    }

    public static List<PaymentItem> CreateValidItems(int count = 1, decimal unitPrice = 99.99m, string currency = "BRL")
    {
        var items = new List<PaymentItem>();
        for (int i = 1; i <= count; i++)
        {
            items.Add(new PaymentItem($"product-{i}", $"Product {i}", 1, Money.FromDecimal(unitPrice, currency)));
        }
        return items;
    }
}
