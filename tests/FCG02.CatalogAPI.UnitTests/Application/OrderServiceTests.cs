using FCG02.CatalogAPI.Application.DTOs;
using FCG02.CatalogAPI.Application.Events;
using FCG02.CatalogAPI.Application.Interfaces;
using FCG02.CatalogAPI.Application.Services;
using FCG02.CatalogAPI.Domain.Entities;
using FCG02.CatalogAPI.Domain.Enums;
using FCG02.CatalogAPI.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace FCG02.CatalogAPI.UnitTests.Application;

public class OrderServiceTests
{
    private readonly Mock<IGameRepository> _gameRepoMock = new();
    private readonly Mock<IUserLibraryRepository> _libRepoMock = new();
    private readonly Mock<IEventPublisher> _publisherMock = new();
    private readonly OrderService _sut;
    public OrderServiceTests()
    {
        _sut = new OrderService(_gameRepoMock.Object, _libRepoMock.Object, _publisherMock.Object);
    }

    [Fact]
    public async Task PlaceOrderAsync_WithValidGameAndNoOwnership_ShouldPublishEventAndReturnOrder()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var game = new Game("Game 1", "Game One", 1, CatalogAPI.Domain.Enums.GameCategory.Action);

        _gameRepoMock.Setup(r => r.GetByIdAsync(game.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        _libRepoMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserLibrary?)null);

        var request = new OrderCreateRequest(game.Id);

        // Act
        var result = await _sut.PlaceOrderAsync(userId, request);

        // Assert
        result.Should().NotBeNull();
        result.GameId.Should().Be(game.Id);
        result.GameTitle.Should().Be(game.Title);
        result.Price.Should().Be(game.Price);
        result.Status.Should().Be("PendingPayment");

        _publisherMock.Verify(p => p.PublishAsync(
            It.Is<OrderPlacedEvent>(e => e.GameId == game.Id && e.UserId == userId && e.Price == game.Price),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PlaceOrderAsync_WhenUserAlreadyOwnsGame_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var game = new Game("Game 1", "Game One", 1, GameCategory.Action);
        var lib = new UserLibrary(userId);
        lib.AddGame(game.Id, game.Title, game.Price);

        _gameRepoMock.Setup(r => r.GetByIdAsync(game.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        _libRepoMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lib);

        var request = new OrderCreateRequest(game.Id);

        // Act
        var act = () => _sut.PlaceOrderAsync(userId, request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already owns the game*");

        _publisherMock.Verify(p => p.PublishAsync(It.IsAny<OrderPlacedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task PlaceOrderAsync_WhenGameIsInactive_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var game = new Game("Old Game", "Inactive game", 50.00m, GameCategory.Action);
        game.Deactivate();

        _gameRepoMock.Setup(r => r.GetByIdAsync(game.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        var request = new OrderCreateRequest(game.Id);

        // Act
        var act = () => _sut.PlaceOrderAsync(userId, request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*inactive for purchase*");

        _publisherMock.Verify(p => p.PublishAsync(It.IsAny<OrderPlacedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}