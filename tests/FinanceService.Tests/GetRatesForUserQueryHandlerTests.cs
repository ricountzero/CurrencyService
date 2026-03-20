using FinanceService.Application.Interfaces;
using FinanceService.Application.Queries;
using FinanceService.Domain.Entities;
using FinanceService.Domain.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

namespace FinanceService.Tests;

public class GetRatesForUserQueryHandlerTests
{
    private readonly Mock<ICurrencyRepository> _currencyRepoMock = new();
    private readonly Mock<IUserFavoritesService> _favoritesMock = new();
    private readonly GetRatesForUserQueryHandler _handler;

    public GetRatesForUserQueryHandlerTests()
    {
        _handler = new GetRatesForUserQueryHandler(
            _currencyRepoMock.Object, _favoritesMock.Object);
    }

    [Fact]
    public async Task Handle_UserHasFavorites_ReturnsCurrencyRates()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var currencyId1 = Guid.NewGuid();
        var currencyId2 = Guid.NewGuid();

        _favoritesMock.Setup(f => f.GetFavoriteCurrencyIdsAsync(userId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new List<Guid> { currencyId1, currencyId2 });

        _currencyRepoMock.Setup(r => r.GetByIdsAsync(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Currency>
            {
                new() { Id = currencyId1, Name = "USD", Rate = 90.5m },
                new() { Id = currencyId2, Name = "EUR", Rate = 99.1m }
            });

        // Act
        var result = await _handler.Handle(new GetRatesForUserQuery(userId), CancellationToken.None);

        // Assert
        result.Rates.Should().HaveCount(2);
        result.Rates.Should().Contain(r => r.Name == "USD" && r.Rate == 90.5m);
        result.Rates.Should().Contain(r => r.Name == "EUR" && r.Rate == 99.1m);
    }

    [Fact]
    public async Task Handle_UserHasNoFavorites_ReturnsEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _favoritesMock.Setup(f => f.GetFavoriteCurrencyIdsAsync(userId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new List<Guid>());

        // Act
        var result = await _handler.Handle(new GetRatesForUserQuery(userId), CancellationToken.None);

        // Assert
        result.Rates.Should().BeEmpty();
        _currencyRepoMock.Verify(
            r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_CallsFavoritesServiceWithCorrectUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _favoritesMock.Setup(f => f.GetFavoriteCurrencyIdsAsync(userId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new List<Guid>());

        // Act
        await _handler.Handle(new GetRatesForUserQuery(userId), CancellationToken.None);

        // Assert
        _favoritesMock.Verify(
            f => f.GetFavoriteCurrencyIdsAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
