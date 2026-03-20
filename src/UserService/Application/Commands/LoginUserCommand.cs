using MediatR;
using Shared.Auth;
using UserService.Application.Interfaces;
using UserService.Domain.Repositories;

namespace UserService.Application.Commands;

public record LoginUserCommand(string Name, string Password) : IRequest<LoginUserResult>;

public record LoginUserResult(string Token);

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, LoginUserResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginUserCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginUserResult> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByNameAsync(request.Name, cancellationToken)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!_passwordHasher.Verify(request.Password, user.Password))
            throw new UnauthorizedAccessException("Invalid credentials.");

        var token = _jwtTokenService.GenerateToken(user.Id, user.Name);
        return new LoginUserResult(token);
    }
}
