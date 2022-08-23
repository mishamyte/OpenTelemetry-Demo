using MassTransit;
using Mu.Client;

namespace Mu.Consumers;

public class GetBarConsumer : IConsumer<GetBar>
{
    public async Task Consume(ConsumeContext<GetBar> context)
    {
        var bar = new Bar(Guid.Parse("49F56898-FC6A-4564-A13F-B1291A6ACC93"), 42);
        await context.RespondAsync(bar);
    }
}