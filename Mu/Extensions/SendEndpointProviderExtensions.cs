using MassTransit;

namespace Mu.Extensions;

public static class SendEndpointProviderExtensions
{
    extension(ISendEndpointProvider sendEndpointProvider)
    {
        public async Task<ISendEndpoint> GetSendEndpoint<T>()
            where T : class
        {
            return await sendEndpointProvider.GetSendEndpoint(
                new Uri($"queue:{KebabCaseEndpointNameFormatter.Instance.Message<T>()}"));
        }
    }
}