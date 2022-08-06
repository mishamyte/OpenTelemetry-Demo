using MediatR;

namespace Sigma.Commands;

public record CreateUserCommand(Guid Id, string Name) : IRequest;