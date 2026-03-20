using FinanceService.Application.Queries;
using FinanceService.Domain.Entities;
using FinanceService.Domain.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

namespace FinanceService.Tests;

public class GetAllRatesQueryHandlerTests
{
    private readonly Mock<ICurrencyRepository> _repoMock = new();
    private readonly GetAllRatesQueryHandler _handler;

    public GetAllRatesQueryHandlerTests()
    {
        _handler = new GetAllRatesQueryHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsMappedCurrencies()
    {
        // Arrange
        _repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Currency>
                 {
                     new() { Id = Guid.NewGuid(), Name = "USD", Rate = 90.0m },
                     new() { Id = Guid.NewGuid(), Name = "CNY", Rate = 12.5m }
                 });

        // Act
        var result = await _handler.Handle(new GetAllRatesQuery(), CancellationToken.None);

        // Assert
        result.Rates.Should().HaveCount(2);
        result.Rates.Should().Contain(r => r.Name == "USD");
        result.Rates.Should().Contain(r => r.Name == "CNY");
    }

    [Fact]
    public async Task Handle_EmptyRepository_ReturnsEmptyList()
    {
        // Arrange
        _repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Currency>());

        // Act
        var result = await _handler.Handle(new GetAllRatesQuery(), CancellationToken.None);

        // Assert
        result.Rates.Should().BeEmpty();
    }
}
