namespace Payment.Domain.AggregatesModel.PaymentAggregate;

public class PaymentItem
{
    public Guid Id { get; private set; }
    public string ProductId { get; private set; }
    public string ProductName { get; private set; }
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; }
    public Money Amount { get; private set; }

    private PaymentItem()
    {
        ProductId = null!;
        ProductName = null!;
        UnitPrice = null!;
        Amount = null!;
    }

    public PaymentItem(
        string productId,
        string productName,
        int quantity,
        Money unitPrice)
    {
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("Product ID is required", nameof(productId));

        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name is required", nameof(productName));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        if (unitPrice.Value <= 0)
            throw new ArgumentException("Unit price must be greater than zero", nameof(unitPrice));

        Id = Guid.NewGuid();
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
        Amount = unitPrice.Multiply(quantity);
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(newQuantity));

        Quantity = newQuantity;
        Amount = UnitPrice.Multiply(newQuantity);
    }

    public void UpdateUnitPrice(Money newUnitPrice)
    {
        if (newUnitPrice.Value <= 0)
            throw new ArgumentException("Unit price must be greater than zero", nameof(newUnitPrice));

        UnitPrice = newUnitPrice;
        Amount = newUnitPrice.Multiply(Quantity);
    }
}
