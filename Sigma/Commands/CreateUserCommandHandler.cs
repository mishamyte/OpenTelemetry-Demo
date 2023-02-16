using MediatR;
using Sigma.Persistence;
using Sigma.Persistence.Entities;

namespace Sigma.Commands;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand>
{
    private readonly SigmaContext _context;

    public CreateUserCommandHandler(SigmaContext context)
    {
        _context = context;
    }

    public async Task Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var entity = new User(request.Id, request.Name);
        await _context.Set<User>().AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}