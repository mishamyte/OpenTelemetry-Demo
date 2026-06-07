using MassTransit;
using Mu.Messages;

namespace Mu.Consumers;

public class CommandConsumer(ILogger<CommandConsumer> logger) : IConsumer<Command>
{
    public Task Consume(ConsumeContext<Command> context)
    {
        logger.LogInformation("Received {@Command}", context.Message);
        return Task.CompletedTask;
    }
}