using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Sigma.Persistence;
using Sigma.Persistence.Entities;

namespace Sigma.Queries;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, GetUserByIdQuery.User?>
{
    private readonly IDistributedCache _cache;
    private readonly SigmaContext _context;

    public GetUserByIdQueryHandler(
        IDistributedCache cache,
        SigmaContext context)
    {
        _cache = cache;
        _context = context;
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