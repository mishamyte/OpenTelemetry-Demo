using MediatR;
using Sigma.Persistence;
using Sigma.Persistence.Entities;

namespace Sigma.Commands;

public class CreateUserCommandHandler : AsyncRequestHandler<CreateUserCommand>
{
    private readonly SigmaContext _context;

    public CreateUserCommandHandler(SigmaContext context)
    {
        _context = context;
    }
    
    protected override async Task Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var entity = new User(request.Id, request.Name);
        await _context.Set<User>().AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}