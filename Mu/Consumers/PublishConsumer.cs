using MassTransit;
using Mu.Messages;

namespace Mu.Consumers;

public class PublishConsumer(ILogger<PublishConsumer> logger) : IConsumer<Publish>
{
    public Task Consume(ConsumeContext<Publish> context)
    {
        logger.LogInformation("Received {@Publish}", context.Message);
        return Task.CompletedTask;
    }
}
