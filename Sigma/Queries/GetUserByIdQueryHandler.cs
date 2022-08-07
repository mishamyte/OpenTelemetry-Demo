using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Sigma.Persistence;
using Sigma.Persistence.Entities;
using StackExchange.Redis;

namespace Sigma.Queries;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, GetUserByIdQuery.User?>
{
    private readonly IDistributedCache _cache;
    private readonly SigmaContext _context;
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public GetUserByIdQueryHandler(
        IDistributedCache cache,
        SigmaContext context,
        IConnectionMultiplexer connectionMultiplexer)
    {
        _cache = cache;
        _context = context;
        _connectionMultiplexer = connectionMultiplexer;
    }

    public async Task<GetUserByIdQuery.User?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        User? entity;

        var entry = await _cache.GetAsync(request.Id.ToString(), cancellationToken);

        if (entry != null)
        {
            entity = JsonSerializer.Deserialize<User>(new ReadOnlySpan<byte>(entry));
        }
        else
        {
            entity = await _context.Set<User>().FirstOrDefaultAsync(user => user.Id == request.Id, cancellationToken);

            if (entity != null)
            {
                var bytes = JsonSerializer.SerializeToUtf8Bytes(entity);
                await _cache.SetAsync(
                    request.Id.ToString(),
                    bytes,
                    new DistributedCacheEntryOptions { SlidingExpiration = TimeSpan.FromSeconds(30) },
                    cancellationToken);
            }
        }

        return entity == null ? null : new GetUserByIdQuery.User(entity.Id, entity.Name);
    }
}