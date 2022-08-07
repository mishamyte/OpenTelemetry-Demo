using MassTransit;
using Mu.Messages;

namespace Mu.Consumers;

public class PublishConsumer : IConsumer<Publish>
{
    private readonly ILogger<PublishConsumer> _logger;

    public PublishConsumer(ILogger<PublishConsumer> logger)
    {
        _logger = logger;
    }
    
    public Task Consume(ConsumeContext<Publish> context)
    {
        _logger.LogInformation("Received {@Publish}", context.Message);
        return Task.CompletedTask;
    }
}