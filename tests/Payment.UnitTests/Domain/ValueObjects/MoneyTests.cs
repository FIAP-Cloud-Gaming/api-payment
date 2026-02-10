namespace Payment.UnitTests.Domain.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void FromDecimal_WithValidValue_ShouldCreateMoney()
    {
        // Act
        var money = Money.FromDecimal(199.99m, "BRL");

        // Assert
        money.Should().NotBeNull();
        money.Value.Should().Be(199.99m);
        money.Currency.Should().Be("BRL");
    }

    [Fact]
    public void FromDecimal_ShouldRoundToTwoDecimals()
    {
        // Act
        var money = Money.FromDecimal(199.999m, "BRL");

        // Assert
        money.Value.Should().Be(200.00m);
    }

    [Fact]
    public void FromDecimal_WithNegativeValue_ShouldThrowException()
    {
        // Act
        var act = () => Money.FromDecimal(-10.00m, "BRL");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Money value cannot be negative*")
            .And.ParamName.Should().Be("value");
    }

    [Fact]
    public void FromDecimal_WithEmptyCurrency_ShouldThrowException()
    {
        // Act
        var act = () => Money.FromDecimal(100.00m, "");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Currency cannot be empty*")
            .And.ParamName.Should().Be("currency");
    }

    [Fact]
    public void FromDecimal_ShouldUppercaseCurrency()
    {
        // Act
        var money = Money.FromDecimal(100.00m, "usd");

        // Assert
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Zero_ShouldCreateZeroMoney()
    {
        // Act
        var money = Money.Zero("BRL");

        // Assert
        money.Value.Should().Be(0m);
        money.Currency.Should().Be("BRL");
    }

    [Fact]
    public void Zero_WithoutCurrency_ShouldDefaultToBRL()
    {
        // Act
        var money = Money.Zero();

        // Assert
        money.Value.Should().Be(0m);
        money.Currency.Should().Be("BRL");
    }

    [Fact]
    public void Add_WithSameCurrency_ShouldAddValues()
    {
        // Arrange
        var money1 = Money.FromDecimal(100.00m, "BRL");
        var money2 = Money.FromDecimal(50.00m, "BRL");

        // Act
        var result = money1.Add(money2);

        // Assert
        result.Value.Should().Be(150.00m);
        result.Currency.Should().Be("BRL");
    }

    [Fact]
    public void Add_WithDifferentCurrency_ShouldThrowException()
    {
        // Arrange
        var money1 = Money.FromDecimal(100.00m, "BRL");
        var money2 = Money.FromDecimal(50.00m, "USD");

        // Act
        var act = () => money1.Add(money2);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot add USD to BRL");
    }

    [Fact]
    public void Subtract_WithSameCurrency_ShouldSubtractValues()
    {
        // Arrange
        var money1 = Money.FromDecimal(100.00m, "BRL");
        var money2 = Money.FromDecimal(30.00m, "BRL");

        // Act
        var result = money1.Subtract(money2);

        // Assert
        result.Value.Should().Be(70.00m);
        result.Currency.Should().Be("BRL");
    }

    [Fact]
    public void Subtract_ResultingInNegative_ShouldThrowException()
    {
        // Arrange
        var money1 = Money.FromDecimal(50.00m, "BRL");
        var money2 = Money.FromDecimal(100.00m, "BRL");

        // Act
        var act = () => money1.Subtract(money2);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot subtract to negative value");
    }

    [Fact]
    public void Subtract_WithDifferentCurrency_ShouldThrowException()
    {
        // Arrange
        var money1 = Money.FromDecimal(100.00m, "BRL");
        var money2 = Money.FromDecimal(50.00m, "USD");

        // Act
        var act = () => money1.Subtract(money2);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot subtract USD from BRL");
    }

    [Fact]
    public void Multiply_WithPositiveMultiplier_ShouldMultiplyValue()
    {
        // Arrange
        var money = Money.FromDecimal(50.00m, "BRL");

        // Act
        var result = money.Multiply(3);

        // Assert
        result.Value.Should().Be(150.00m);
        result.Currency.Should().Be("BRL");
    }

    [Fact]
    public void Multiply_WithNegativeMultiplier_ShouldThrowException()
    {
        // Arrange
        var money = Money.FromDecimal(50.00m, "BRL");

        // Act
        var act = () => money.Multiply(-2);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Multiplier cannot be negative*")
            .And.ParamName.Should().Be("multiplier");
    }

    [Fact]
    public void IsGreaterThan_WhenGreater_ShouldReturnTrue()
    {
        // Arrange
        var money1 = Money.FromDecimal(100.00m, "BRL");
        var money2 = Money.FromDecimal(50.00m, "BRL");

        // Act & Assert
        money1.IsGreaterThan(money2).Should().BeTrue();
    }

    [Fact]
    public void IsGreaterThan_WhenEqual_ShouldReturnFalse()
    {
        // Arrange
        var money1 = Money.FromDecimal(100.00m, "BRL");
        var money2 = Money.FromDecimal(100.00m, "BRL");

        // Act & Assert
        money1.IsGreaterThan(money2).Should().BeFalse();
    }

    [Fact]
    public void IsGreaterThan_WhenLess_ShouldReturnFalse()
    {
        // Arrange
        var money1 = Money.FromDecimal(50.00m, "BRL");
        var money2 = Money.FromDecimal(100.00m, "BRL");

        // Act & Assert
        money1.IsGreaterThan(money2).Should().BeFalse();
    }

    [Fact]
    public void IsGreaterThan_WithDifferentCurrency_ShouldThrowException()
    {
        // Arrange
        var money1 = Money.FromDecimal(100.00m, "BRL");
        var money2 = Money.FromDecimal(50.00m, "USD");

        // Act
        var act = () => money1.IsGreaterThan(money2);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot compare USD with BRL");
    }

    [Fact]
    public void IsLessThan_WhenLess_ShouldReturnTrue()
    {
        // Arrange
        var money1 = Money.FromDecimal(50.00m, "BRL");
        var money2 = Money.FromDecimal(100.00m, "BRL");

        // Act & Assert
        money1.IsLessThan(money2).Should().BeTrue();
    }

    [Fact]
    public void IsLessThan_WhenEqual_ShouldReturnFalse()
    {
        // Arrange
        var money1 = Money.FromDecimal(100.00m, "BRL");
        var money2 = Money.FromDecimal(100.00m, "BRL");

        // Act & Assert
        money1.IsLessThan(money2).Should().BeFalse();
    }

    [Fact]
    public void IsLessThan_WhenGreater_ShouldReturnFalse()
    {
        // Arrange
        var money1 = Money.FromDecimal(100.00m, "BRL");
        var money2 = Money.FromDecimal(50.00m, "BRL");

        // Act & Assert
        money1.IsLessThan(money2).Should().BeFalse();
    }

    [Fact]
    public void IsLessThan_WithDifferentCurrency_ShouldThrowException()
    {
        // Arrange
        var money1 = Money.FromDecimal(100.00m, "BRL");
        var money2 = Money.FromDecimal(50.00m, "USD");

        // Act
        var act = () => money1.IsLessThan(money2);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot compare USD with BRL");
    }

    [Fact]
    public void ToString_ShouldFormatCorrectly()
    {
        // Arrange
        var money = Money.FromDecimal(1234.56m, "BRL");

        // Act
        var result = money.ToString();

        // Assert
        result.Should().Be("BRL 1,234.56");
    }

    [Fact]
    public void Equality_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var money1 = Money.FromDecimal(100.00m, "BRL");
        var money2 = Money.FromDecimal(100.00m, "BRL");

        // Act & Assert
        money1.Should().Be(money2);
        (money1 == money2).Should().BeTrue();
    }

    [Fact]
    public void Equality_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var money1 = Money.FromDecimal(100.00m, "BRL");
        var money2 = Money.FromDecimal(50.00m, "BRL");

        // Act & Assert
        money1.Should().NotBe(money2);
        (money1 != money2).Should().BeTrue();
    }

    [Fact]
    public void Equality_WithDifferentCurrency_ShouldNotBeEqual()
    {
        // Arrange
        var money1 = Money.FromDecimal(100.00m, "BRL");
        var money2 = Money.FromDecimal(100.00m, "USD");

        // Act & Assert
        money1.Should().NotBe(money2);
        (money1 != money2).Should().BeTrue();
    }
}
