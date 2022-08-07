using MassTransit;

namespace Mu.Extensions;

public static class SendEndpointProviderExtensions
{
    public static async Task<ISendEndpoint> GetSendEndpoint<T>(
        this ISendEndpointProvider sendEndpointProvider)
        where T : class =>
        await sendEndpointProvider.GetSendEndpoint(
            new Uri($"queue:{KebabCaseEndpointNameFormatter.Instance.Message<T>()}"));
}