using MassTransit;
using Mu.Messages;

namespace Mu.Consumers;

public class CommandConsumer : IConsumer<Command>
{
    private readonly ILogger<CommandConsumer> _logger;

    public CommandConsumer(ILogger<CommandConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<Command> context)
    {
        _logger.LogInformation("Received {@Command}", context.Message);
        return Task.CompletedTask;
    }
}