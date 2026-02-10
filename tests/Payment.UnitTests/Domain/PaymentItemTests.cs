namespace Payment.UnitTests.Domain;

public class PaymentItemTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateItemSuccessfully()
    {
        // Arrange
        var productId = "product-123";
        var productName = "Test Product";
        var quantity = 2;
        var unitPrice = Money.FromDecimal(99.99m, "BRL");

        // Act
        var item = new PaymentItem(productId, productName, quantity, unitPrice);

        // Assert
        item.Should().NotBeNull();
        item.Id.Should().NotBeEmpty();
        item.ProductId.Should().Be(productId);
        item.ProductName.Should().Be(productName);
        item.Quantity.Should().Be(quantity);
        item.UnitPrice.Should().Be(unitPrice);
        item.Amount.Value.Should().Be(199.98m); // 2 * 99.99
        item.Amount.Currency.Should().Be("BRL");
    }

    [Fact]
    public void Constructor_WithEmptyProductId_ShouldThrowException()
    {
        // Arrange
        var unitPrice = Money.FromDecimal(99.99m, "BRL");

        // Act
        var act = () => new PaymentItem("", "Product Name", 1, unitPrice);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Product ID is required*")
            .And.ParamName.Should().Be("productId");
    }

    [Fact]
    public void Constructor_WithEmptyProductName_ShouldThrowException()
    {
        // Arrange
        var unitPrice = Money.FromDecimal(99.99m, "BRL");

        // Act
        var act = () => new PaymentItem("product-123", "", 1, unitPrice);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Product name is required*")
            .And.ParamName.Should().Be("productName");
    }

    [Fact]
    public void Constructor_WithZeroQuantity_ShouldThrowException()
    {
        // Arrange
        var unitPrice = Money.FromDecimal(99.99m, "BRL");

        // Act
        var act = () => new PaymentItem("product-123", "Product Name", 0, unitPrice);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Quantity must be greater than zero*")
            .And.ParamName.Should().Be("quantity");
    }

    [Fact]
    public void Constructor_WithNegativeQuantity_ShouldThrowException()
    {
        // Arrange
        var unitPrice = Money.FromDecimal(99.99m, "BRL");

        // Act
        var act = () => new PaymentItem("product-123", "Product Name", -1, unitPrice);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Quantity must be greater than zero*")
            .And.ParamName.Should().Be("quantity");
    }

    [Fact]
    public void Constructor_WithZeroUnitPrice_ShouldThrowException()
    {
        // Arrange
        var unitPrice = Money.FromDecimal(0m, "BRL");

        // Act
        var act = () => new PaymentItem("product-123", "Product Name", 1, unitPrice);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Unit price must be greater than zero*")
            .And.ParamName.Should().Be("unitPrice");
    }

    [Fact]
    public void UpdateQuantity_WithValidQuantity_ShouldUpdateQuantityAndAmount()
    {
        // Arrange
        var unitPrice = Money.FromDecimal(50.00m, "BRL");
        var item = new PaymentItem("product-123", "Product Name", 1, unitPrice);

        // Act
        item.UpdateQuantity(3);

        // Assert
        item.Quantity.Should().Be(3);
        item.Amount.Value.Should().Be(150.00m); // 3 * 50.00
        item.UnitPrice.Should().Be(unitPrice); // Unchanged
    }

    [Fact]
    public void UpdateQuantity_WithZero_ShouldThrowException()
    {
        // Arrange
        var unitPrice = Money.FromDecimal(50.00m, "BRL");
        var item = new PaymentItem("product-123", "Product Name", 1, unitPrice);

        // Act
        var act = () => item.UpdateQuantity(0);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Quantity must be greater than zero*")
            .And.ParamName.Should().Be("newQuantity");
    }

    [Fact]
    public void UpdateQuantity_WithNegative_ShouldThrowException()
    {
        // Arrange
        var unitPrice = Money.FromDecimal(50.00m, "BRL");
        var item = new PaymentItem("product-123", "Product Name", 1, unitPrice);

        // Act
        var act = () => item.UpdateQuantity(-5);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Quantity must be greater than zero*")
            .And.ParamName.Should().Be("newQuantity");
    }

    [Fact]
    public void UpdateUnitPrice_WithValidPrice_ShouldUpdateUnitPriceAndAmount()
    {
        // Arrange
        var initialPrice = Money.FromDecimal(50.00m, "BRL");
        var item = new PaymentItem("product-123", "Product Name", 2, initialPrice);
        var newPrice = Money.FromDecimal(75.00m, "BRL");

        // Act
        item.UpdateUnitPrice(newPrice);

        // Assert
        item.UnitPrice.Should().Be(newPrice);
        item.Amount.Value.Should().Be(150.00m); // 2 * 75.00
        item.Quantity.Should().Be(2); // Unchanged
    }

    [Fact]
    public void UpdateUnitPrice_WithZeroPrice_ShouldThrowException()
    {
        // Arrange
        var initialPrice = Money.FromDecimal(50.00m, "BRL");
        var item = new PaymentItem("product-123", "Product Name", 1, initialPrice);
        var zeroPrice = Money.FromDecimal(0m, "BRL");

        // Act
        var act = () => item.UpdateUnitPrice(zeroPrice);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Unit price must be greater than zero*")
            .And.ParamName.Should().Be("newUnitPrice");
    }
}
