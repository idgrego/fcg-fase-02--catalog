using System.Formats.Asn1;
using FCG02.CatalogAPI.Domain.Entities;
using FCG02.CatalogAPI.Domain.Enums;
using FluentAssertions;

namespace FCG02.CatalogAPI.UnitTests.Domain;

public class GameTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateGameSuccessfully()
    {
        // Arrange
        var title = "Cyberpunk Odyssey";
        var description = "An immersive open-world sci-fi RPG";
        var price = 199.90m; // decimal
        var category = GameCategory.RPG;
        var tags = new[] { "Open World", "Sci-Fi", "RPG" };

        // Act
        var game = new Game(title, description, price, category, tags);

        // Assert
        game.Id.Should().NotBeEmpty();
        game.Title.Should().Be(title);
        game.Description.Should().Be(description);
        game.Price.Should().Be(price);
        game.Tags.Should().Contain(tags.Select(t => t.Trim().ToLowerInvariant()).ToArray());
        game.IsActive.Should().BeTrue();
        game.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        game.UpdatedAt.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithInvalidTitle_ShouldThrowArgumentException(string? invalidTitle)
    {
        // Arrange
        var description = "An immersive open-world sci-fi RPG";
        var price = 199.90m; // decimal
        var category = GameCategory.RPG;

        // Act
        var act = () => new Game(invalidTitle!, description, price, category);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Title cannot be null or empty. (Parameter 'title')");
    }

    [Fact]
    public void Constructor_WithNegativePrice_ShouldThrowArgumentoException()
    {
        // Arrange
        var title = "Cyberpunk Odyssey";
        var description = "An immersive open-world sci-fi RPG";
        var category = GameCategory.RPG;

        // Act
        var act = () => new Game(title, description, -1, category);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Price cannot be negative. (Parameter 'price')");
    }

    [Fact]
    public void UpdateDetails_WithValidParameters_ShouldUpdatePropertiesAndSetUpdatedAt()
    {
        // Arrange
        var game = new Game("Original title", "Original description", 1m, GameCategory.Action);
        var newTitle = "New title";
        var newDescription = "New description";
        var newPrice = 2m;
        var newCat = GameCategory.Adventure;
        var newTags = new[] { "New Tag 1", "New Tag 2" };

        // Act
        game.UpdateDetails(newTitle, newDescription, newPrice, newCat, newTags);

        // Assert
        game.Title.Should().Be(newTitle);
        game.Description.Should().Be(newDescription);
        game.Price.Should().Be(newPrice);
        game.Category.Should().Be(newCat);
        game.Tags.Should().Contain(newTags.Select(t => t.Trim().ToLowerInvariant()).ToArray());
        game.UpdatedAt.Should().NotBeNull();
        game.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var game = new Game("Valid Title", "Description", 100m, GameCategory.Simulation);

        // Act
        game.Deactivate();

        // Assert
        game.IsActive.Should().BeFalse();
        game.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Activate_WhenDeactivated_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var game = new Game("Valid Title", "Description", 100m, GameCategory.Simulation);
        game.Deactivate();

        // Act
        game.Activate();

        // Assert
        game.IsActive.Should().BeTrue();
    }
}
