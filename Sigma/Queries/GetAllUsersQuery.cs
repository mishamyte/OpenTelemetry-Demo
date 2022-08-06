using MediatR;

namespace Sigma.Queries;

public class GetAllUsersQuery : IRequest<IEnumerable<GetAllUsersQuery.User>>
{
    public record User(Guid Id, string Name);
}