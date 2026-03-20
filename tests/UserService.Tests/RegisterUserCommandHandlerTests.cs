using FluentAssertions;
using Moq;
using UserService.Application.Commands;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;
using Xunit;

namespace UserService.Tests;

public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _repoMock = new();
    private readonly Mock<IPasswordHasher> _hasherMock = new();
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        _handler = new RegisterUserCommandHandler(_repoMock.Object, _hasherMock.Object);
    }

    [Fact]
    public async Task Handle_NewUser_ReturnsResult()
    {
        // Arrange
        _repoMock.Setup(r => r.GetByNameAsync("alice", It.IsAny<CancellationToken>()))
                 .ReturnsAsync((User?)null);
        _hasherMock.Setup(h => h.Hash("secret")).Returns("hashed");
        _repoMock.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

        var command = new RegisterUserCommand("alice", "secret");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Name.Should().Be("alice");
        result.UserId.Should().NotBeEmpty();
        _repoMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateName_ThrowsInvalidOperationException()
    {
        // Arrange
        var existing = User.Create("alice", "hashed");
        _repoMock.Setup(r => r.GetByNameAsync("alice", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existing);

        var command = new RegisterUserCommand("alice", "secret");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
        _repoMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_HashesPasswordBeforeSaving()
    {
        // Arrange
        _repoMock.Setup(r => r.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((User?)null);
        _hasherMock.Setup(h => h.Hash("plain")).Returns("$2a$hashed");
        _repoMock.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

        var command = new RegisterUserCommand("bob", "plain");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _hasherMock.Verify(h => h.Hash("plain"), Times.Once);
        _repoMock.Verify(r => r.AddAsync(
            It.Is<User>(u => u.Password == "$2a$hashed"),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
