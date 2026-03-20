using FinanceService.Application.Commands;
using FinanceService.Application.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace FinanceService.Tests;

public class AddFavoriteCurrencyCommandHandlerTests
{
    private readonly Mock<IUserFavoritesService> _favoritesMock = new();
    private readonly AddFavoriteCurrencyCommandHandler _handler;

    public AddFavoriteCurrencyCommandHandlerTests()
    {
        _handler = new AddFavoriteCurrencyCommandHandler(_favoritesMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsAddFavorite()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var currencyId = Guid.NewGuid();
        _favoritesMock.Setup(f => f.AddFavoriteCurrencyAsync(userId, currencyId, It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(new AddFavoriteCurrencyCommand(userId, currencyId), CancellationToken.None);

        // Assert
        _favoritesMock.Verify(
            f => f.AddFavoriteCurrencyAsync(userId, currencyId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ServiceThrows_ExceptionPropagates()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var currencyId = Guid.NewGuid();
        _favoritesMock.Setup(f => f.AddFavoriteCurrencyAsync(userId, currencyId, It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new InvalidOperationException("DB error"));

        // Act
        var act = () => _handler.Handle(new AddFavoriteCurrencyCommand(userId, currencyId), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("DB error");
    }
}
