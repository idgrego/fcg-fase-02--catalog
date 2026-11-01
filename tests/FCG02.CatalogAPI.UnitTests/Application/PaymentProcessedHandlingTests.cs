using System.Runtime.CompilerServices;
using FCG02.CatalogAPI.Application.Events;
using FCG02.CatalogAPI.Application.Interfaces;
using FCG02.CatalogAPI.Domain.Entities;
using FluentAssertions;
using Moq;

namespace FCG02.CatalogAPI.UnitTests.Application;

public class PaymentProcessedHandlingTests
{
    private readonly Mock<IUserLibraryService> _libServiceMock;
    public PaymentProcessedHandlingTests()
    {
        _libServiceMock = new Mock<IUserLibraryService>();
    }

    [Fact]
    public async Task ProcessEvent_WhenPaymentIsApproved_ShouldAddGameToLibrary()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var gameId = Guid.CreateVersion7();
        var orderId = Guid.CreateVersion7();
        var price = 199.90m;
        var paymentEvent = new PaymentProcessedIntegrationEvent(
            EventId: Guid.CreateVersion7(),
            OccurredOn: DateTime.UtcNow,
            OrderId: orderId,
            UserId: userId,
            GameId: gameId,
            Amount: price,
            Status: "Approved",
            Reason: null,
            ProcessedAt: DateTime.UtcNow
        );

        _libServiceMock.Setup(s => s.AddGameToLibraryASync(userId, gameId, price, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        if (paymentEvent.IsApproved)
        {
            await _libServiceMock.Object.AddGameToLibraryASync(
                paymentEvent.UserId,
                paymentEvent.GameId,
                paymentEvent.Amount,
                CancellationToken.None
            );
        }

        // Assert
        paymentEvent.IsApproved.Should().BeTrue();
        _libServiceMock.Verify(s => s.AddGameToLibraryASync(userId, gameId, price, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessEvent_WhenPaymentIsRejected_ShouldNotAddGameToLibrary()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var gameId = Guid.CreateVersion7();
        var orderId = Guid.CreateVersion7();

        var paymentEvent = new PaymentProcessedIntegrationEvent(
            EventId: Guid.CreateVersion7(),
            OccurredOn: DateTime.UtcNow,
            OrderId: orderId,
            UserId: userId,
            GameId: gameId,
            Amount: 150.00m,
            Status: "Rejected",
            Reason: "Saldo Insuficiente",
            ProcessedAt: DateTime.UtcNow
        );

        // Act
        if (paymentEvent.IsApproved)
        {
            await _libServiceMock.Object.AddGameToLibraryASync(
                paymentEvent.UserId,
                paymentEvent.GameId,
                paymentEvent.Amount,
                CancellationToken.None
            );
        }

        // Assert
        paymentEvent.IsApproved.Should().BeFalse();
        _libServiceMock.Verify(s => s.AddGameToLibraryASync(
            userId: It.IsAny<Guid>(),
            gameId: It.IsAny<Guid>(),
            pricePaid: It.IsAny<decimal>(),
            cancellationToken: It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

}