using Dapper;
using MediatR;
using Npgsql;
using Sigma.Persistence.Entities;

namespace Sigma.Queries;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, IEnumerable<GetAllUsersQuery.User>>
{
    private readonly IConfiguration _configuration;

    public GetAllUsersQueryHandler(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public async Task<IEnumerable<GetAllUsersQuery.User>> Handle(
        GetAllUsersQuery request,
        CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_configuration.GetConnectionString("Sigma"));
        var entities = await connection.QueryAsync<User>("SELECT * FROM \"User\"");
        return entities.Select(user => new GetAllUsersQuery.User(user.Id, user.Name));
    }
}