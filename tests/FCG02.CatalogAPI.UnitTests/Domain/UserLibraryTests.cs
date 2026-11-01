using FCG02.CatalogAPI.Domain.Entities;
using FluentAssertions;

namespace FCG02.CatalogAPI.UnitTests.Domain;

public class UserLibraryTests
{
    [Fact]
    public void Constructor_WithValidUserId_ShouldInitializeEmptyLibray()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var library = new UserLibrary(userId);

        // Assert
        library.Id.Should().NotBeEmpty();
        library.UserId.Should().Be(userId);
        library.Items.Should().BeEmpty();
        library.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        library.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Contructor_WithEmptyUserId_ShouldThrowArgumentException()
    {
        // Arrange
        var userId = Guid.Empty;

        // Act
        var act = () => new UserLibrary(userId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("UserId cannot be empty (Parameter 'userId')");
    }

    [Fact]
    public void AddGame_WithNewGame_ShouldAddItemToLibrary()
    {
        // Arrange
        var library = new UserLibrary(userId: Guid.NewGuid());
        var game = new Game("Game 1", "Game number one", 1m, CatalogAPI.Domain.Enums.GameCategory.Action);

        // Act
        library.AddGame(game.Id, game.Title, game.Price);

        // Assert
        library.Items.Should().HaveCount(1);
        library.HasGame(game.Id).Should().BeTrue();
        library.UpdatedAt.Should().NotBeNull();

        var item = library.Items.First();
        item.GameId.Should().Be(game.Id);
        item.AcquiredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void AddGame_WhenGameAlreadyInLibrary_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var library = new UserLibrary(Guid.NewGuid());
        var gameId = Guid.NewGuid();
        library.AddGame(gameId, "Hollow Knight", 46.99m);

        // Act
        var act = () => library.AddGame(gameId, "Hollow Knight", 46.99m);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*is already in user library*");
    }

    [Fact]
    public void AddGame_WithNegativePrice_ShouldThrowArgumentException()
    {
        // Arrange
        var library = new UserLibrary(Guid.NewGuid());

        // Act
        var act = () => library.AddGame(Guid.NewGuid(), "Game Title", -5m);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*price paid cannot be negative*");
        //.WithMessage("Price paid cannot be negative (Parameter 'pricePaid')");
    }
}