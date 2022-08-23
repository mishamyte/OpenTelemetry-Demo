using MassTransit;

namespace Shared.MassTransit;

public abstract class BaseConsumerDefinition<TConsumer> : ConsumerDefinition<TConsumer>
    where TConsumer : class, IConsumer
{
    protected BaseConsumerDefinition()
    {
        EndpointName = KebabCaseEndpointNameFormatter.Instance.SanitizeName(typeof(TConsumer).FullName!);
    }
}