using FluentAssertions;
using Moq;
using Shared.Auth;
using UserService.Application.Commands;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;
using Xunit;

namespace UserService.Tests;

public class LoginUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _repoMock = new();
    private readonly Mock<IPasswordHasher> _hasherMock = new();
    private readonly Mock<IJwtTokenService> _jwtMock = new();
    private readonly LoginUserCommandHandler _handler;

    public LoginUserCommandHandlerTests()
    {
        _handler = new LoginUserCommandHandler(
            _repoMock.Object, _hasherMock.Object, _jwtMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsToken()
    {
        // Arrange
        var user = User.Create("alice", "hashed");
        _repoMock.Setup(r => r.GetByNameAsync("alice", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify("plain", "hashed")).Returns(true);
        _jwtMock.Setup(j => j.GenerateToken(user.Id, "alice")).Returns("jwt-token");

        var command = new LoginUserCommand("alice", "plain");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Token.Should().Be("jwt-token");
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsUnauthorized()
    {
        // Arrange
        _repoMock.Setup(r => r.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((User?)null);

        var command = new LoginUserCommand("ghost", "pass");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_WrongPassword_ThrowsUnauthorized()
    {
        // Arrange
        var user = User.Create("alice", "hashed");
        _repoMock.Setup(r => r.GetByNameAsync("alice", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify("wrong", "hashed")).Returns(false);

        var command = new LoginUserCommand("alice", "wrong");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_ValidCredentials_CallsJwtGenerateOnce()
    {
        // Arrange
        var user = User.Create("alice", "hashed");
        _repoMock.Setup(r => r.GetByNameAsync("alice", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify("plain", "hashed")).Returns(true);
        _jwtMock.Setup(j => j.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>())).Returns("token");

        // Act
        await _handler.Handle(new LoginUserCommand("alice", "plain"), CancellationToken.None);

        // Assert
        _jwtMock.Verify(j => j.GenerateToken(user.Id, "alice"), Times.Once);
    }
}
