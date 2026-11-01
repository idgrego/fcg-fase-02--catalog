using System.Runtime.CompilerServices;
using FCG02.CatalogAPI.Application.DTOs;
using FCG02.CatalogAPI.Application.Services;
using FCG02.CatalogAPI.Domain.Entities;
using FCG02.CatalogAPI.Domain.Enums;
using FCG02.CatalogAPI.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace FCG02.CatalogAPI.UnitTests.Application;

public class GameServiceTests
{
    private readonly Mock<IGameRepository> _gameRepoMock = new Mock<IGameRepository>();
    private readonly GameService _sut;

    public GameServiceTests()
    {
        _sut = new GameService(_gameRepoMock.Object);
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_ShouldAddAndReturnGameResponse()
    {
        // Arrange
        var request = new GameCreateRequest(
            Title: "Game #1",
            Description: "Game One",
            Price: 1m,
            Category: GameCategory.Action,
            Tags: new List<String> { "Tag 1", "Tag2 " }
        );

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(request.Title);
        result.Price.Should().Be(request.Price);
        result.Category.Should().Be(request.Category);
        _gameRepoMock.Verify(r =>
            r.AddAsync(
                It.Is<Game>(g => g.Title == request.Title),
                It.IsAny<CancellationToken>()),
                Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenGameNotFound_ShouldReturnNull()
    {
        // Arrange
        _gameRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Game?)null);

        // Act
        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }
}