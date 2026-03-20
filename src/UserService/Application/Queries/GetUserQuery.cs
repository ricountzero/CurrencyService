using MediatR;
using UserService.Domain.Repositories;

namespace UserService.Application.Queries;

public record GetUserQuery(Guid UserId) : IRequest<GetUserResult>;

public record GetUserResult(Guid Id, string Name, List<Guid> FavoriteCurrencyIds);

public class GetUserQueryHandler : IRequestHandler<GetUserQuery, GetUserResult>
{
    private readonly IUserRepository _userRepository;

    public GetUserQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<GetUserResult> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException($"User {request.UserId} not found.");

        return new GetUserResult(user.Id, user.Name, user.FavoriteCurrencyIds);
    }
}
