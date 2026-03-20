using MediatR;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;

namespace UserService.Application.Commands;

public record RegisterUserCommand(string Name, string Password) : IRequest<RegisterUserResult>;

public record RegisterUserResult(Guid UserId, string Name);

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterUserResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<RegisterUserResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var existing = await _userRepository.GetByNameAsync(request.Name, cancellationToken);
        if (existing is not null)
            throw new InvalidOperationException($"User with name '{request.Name}' already exists.");

        var hashedPassword = _passwordHasher.Hash(request.Password);
        var user = User.Create(request.Name, hashedPassword);

        await _userRepository.AddAsync(user, cancellationToken);

        return new RegisterUserResult(user.Id, user.Name);
    }
}
