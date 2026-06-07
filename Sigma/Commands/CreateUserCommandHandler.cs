using MediatR;
using Sigma.Persistence;
using Sigma.Persistence.Entities;

namespace Sigma.Commands;

public class CreateUserCommandHandler(SigmaContext context) : IRequestHandler<CreateUserCommand>
{
    public async Task Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var entity = new User(request.Id, request.Name);
        await context.Set<User>().AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}