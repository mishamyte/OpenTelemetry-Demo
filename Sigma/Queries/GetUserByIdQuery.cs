using MediatR;

namespace Sigma.Queries;

public class GetUserByIdQuery : IRequest<GetUserByIdQuery.User?>
{
    public Guid Id { get; set; }
    
    public record User(Guid Id, string Name);
}