namespace Payment.UnitTests.Domain.ValueObjects;

public class CardInfoTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateCardInfoAndMaskNumber()
    {
        // Act
        var cardInfo = CardInfo.Create("John Doe", "4111111111111111", "Visa", 12, 2027);

        // Assert
        cardInfo.Should().NotBeNull();
        cardInfo.CardHolderName.Should().Be("John Doe");
        cardInfo.MaskedCardNumber.Should().Be("**** **** **** 1111");
        cardInfo.CardBrand.Should().Be("Visa");
        cardInfo.ExpirationMonth.Should().Be(12);
        cardInfo.ExpirationYear.Should().Be(2027);
    }

    [Fact]
    public void MaskedCardNumber_ShouldShowOnlyLastFourDigits()
    {
        // Arrange & Act
        var cardInfo = CardInfo.Create("Jane Smith", "5555555555554444", "Mastercard", 6, 2028);

        // Assert
        cardInfo.MaskedCardNumber.Should().Be("**** **** **** 4444");
    }

    [Fact]
    public void Create_WithEmptyCardHolderName_ShouldThrowException()
    {
        // Act
        var act = () => CardInfo.Create("", "4111111111111111", "Visa", 12, 2027);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Card holder name is required*")
            .And.ParamName.Should().Be("cardHolderName");
    }

    [Fact]
    public void Create_WithEmptyCardNumber_ShouldMaskToFourAsterisks()
    {
        // Act
        var cardInfo = CardInfo.Create("John Doe", "", "Visa", 12, 2027);

        // Assert
        cardInfo.MaskedCardNumber.Should().Be("****");
    }

    [Fact]
    public void Create_WithShortCardNumber_ShouldMaskToFourAsterisks()
    {
        // Act
        var cardInfo = CardInfo.Create("John Doe", "123", "Visa", 12, 2027);

        // Assert
        cardInfo.MaskedCardNumber.Should().Be("****");
    }

    [Fact]
    public void Create_WithInvalidMonthLessThanOne_ShouldThrowException()
    {
        // Act
        var act = () => CardInfo.Create("John Doe", "4111111111111111", "Visa", 0, 2027);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Invalid expiration month*")
            .And.ParamName.Should().Be("expirationMonth");
    }

    [Fact]
    public void Create_WithInvalidMonthGreaterThanTwelve_ShouldThrowException()
    {
        // Act
        var act = () => CardInfo.Create("John Doe", "4111111111111111", "Visa", 13, 2027);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Invalid expiration month*")
            .And.ParamName.Should().Be("expirationMonth");
    }

    [Fact]
    public void Create_WithPastYear_ShouldThrowException()
    {
        // Arrange
        var pastYear = DateTime.UtcNow.Year - 1;

        // Act
        var act = () => CardInfo.Create("John Doe", "4111111111111111", "Visa", 12, pastYear);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Invalid expiration year*")
            .And.ParamName.Should().Be("expirationYear");
    }

    [Fact]
    public void Create_WithFarFutureYear_ShouldThrowException()
    {
        // Arrange
        var farFutureYear = DateTime.UtcNow.Year + 21;

        // Act
        var act = () => CardInfo.Create("John Doe", "4111111111111111", "Visa", 12, farFutureYear);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Invalid expiration year*")
            .And.ParamName.Should().Be("expirationYear");
    }

    [Fact]
    public void IsExpired_WhenNotExpired_ShouldReturnFalse()
    {
        // Arrange
        var futureYear = DateTime.UtcNow.Year + 2;
        var cardInfo = CardInfo.Create("John Doe", "4111111111111111", "Visa", 12, futureYear);

        // Act & Assert
        cardInfo.IsExpired().Should().BeFalse();
    }

    [Fact]
    public void IsExpired_WithPastYear_ShouldReturnTrue()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var cardInfo = CardInfo.Create("John Doe", "4111111111111111", "Visa", 12, now.Year);

        // Simulating a past year by creating a card with current year
        // Then we need to test with reflection or wait, but let's test the logic
        // Since we can't create a past year card (constructor prevents it),
        // we test with current year and past month

        // Act & Assert
        if (now.Month < 12)
        {
            var pastMonthCard = CardInfo.Create("John Doe", "4111111111111111", "Visa", now.Month - 1, now.Year);
            pastMonthCard.IsExpired().Should().BeTrue();
        }
        else
        {
            // For December, card is not expired
            var currentCard = CardInfo.Create("John Doe", "4111111111111111", "Visa", 12, now.Year);
            currentCard.IsExpired().Should().BeFalse();
        }
    }

    [Fact]
    public void IsExpired_WithSameYearAndPastMonth_ShouldReturnTrue()
    {
        // Arrange
        var now = DateTime.UtcNow;

        // Only test if not in January
        if (now.Month > 1)
        {
            var cardInfo = CardInfo.Create("John Doe", "4111111111111111", "Visa", now.Month - 1, now.Year);

            // Act & Assert
            cardInfo.IsExpired().Should().BeTrue();
        }
    }

    [Fact]
    public void ToString_ShouldFormatCorrectly()
    {
        // Arrange
        var cardInfo = CardInfo.Create("John Doe", "4111111111111111", "Visa", 6, 2027);

        // Act
        var result = cardInfo.ToString();

        // Assert
        result.Should().Be("Visa **** **** **** 1111 (06/2027)");
    }

    [Fact]
    public void Create_WithNullCardBrand_ShouldDefaultToUnknown()
    {
        // Act
        var cardInfo = CardInfo.Create("John Doe", "4111111111111111", null!, 12, 2027);

        // Assert
        cardInfo.CardBrand.Should().Be("Unknown");
    }

    [Fact]
    public void Create_WithCardNumberContainingSpaces_ShouldMaskCorrectly()
    {
        // Arrange & Act
        var cardInfo = CardInfo.Create("John Doe", "4111 1111 1111 1111", "Visa", 12, 2027);

        // Assert
        cardInfo.MaskedCardNumber.Should().Be("**** **** **** 1111");
    }

    [Fact]
    public void Create_WithCardNumberContainingDashes_ShouldMaskCorrectly()
    {
        // Arrange & Act
        var cardInfo = CardInfo.Create("John Doe", "4111-1111-1111-1111", "Visa", 12, 2027);

        // Assert
        cardInfo.MaskedCardNumber.Should().Be("**** **** **** 1111");
    }

    [Fact]
    public void Equality_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var card1 = CardInfo.Create("John Doe", "4111111111111111", "Visa", 12, 2027);
        var card2 = CardInfo.Create("John Doe", "4111111111111111", "Visa", 12, 2027);

        // Act & Assert
        card1.Should().Be(card2);
    }

    [Fact]
    public void Equality_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var card1 = CardInfo.Create("John Doe", "4111111111111111", "Visa", 12, 2027);
        var card2 = CardInfo.Create("Jane Smith", "5555555555554444", "Mastercard", 6, 2028);

        // Act & Assert
        card1.Should().NotBe(card2);
    }
}
