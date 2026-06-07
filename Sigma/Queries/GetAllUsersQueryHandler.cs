using Dapper;
using MediatR;
using Npgsql;
using Sigma.Persistence.Entities;

namespace Sigma.Queries;

public class GetAllUsersQueryHandler(IConfiguration configuration)
    : IRequestHandler<GetAllUsersQuery, IEnumerable<GetAllUsersQuery.User>>
{
    public async Task<IEnumerable<GetAllUsersQuery.User>> Handle(
        GetAllUsersQuery request,
        CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(configuration.GetConnectionString("Sigma"));
        var entities = await connection.QueryAsync<User>("SELECT * FROM \"User\"");
        return entities.Select(user => new GetAllUsersQuery.User(user.Id, user.Name));
    }
}
