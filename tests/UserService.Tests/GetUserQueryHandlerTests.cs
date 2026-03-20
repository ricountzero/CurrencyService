using FluentAssertions;
using Moq;
using UserService.Application.Queries;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;
using Xunit;

namespace UserService.Tests;

public class GetUserQueryHandlerTests
{
    private readonly Mock<IUserRepository> _repoMock = new();
    private readonly GetUserQueryHandler _handler;

    public GetUserQueryHandlerTests()
    {
        _handler = new GetUserQueryHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingUser_ReturnsResult()
    {
        // Arrange
        var user = User.Create("alice", "hashed");
        _repoMock.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(new GetUserQuery(user.Id), CancellationToken.None);

        // Assert
        result.Id.Should().Be(user.Id);
        result.Name.Should().Be("alice");
    }

    [Fact]
    public async Task Handle_NonExistingUser_ThrowsKeyNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((User?)null);

        // Act
        var act = () => _handler.Handle(new GetUserQuery(id), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
