using MassTransit;

namespace Mu.Client;

public class MuClient(IClientFactory clientFactory) : IMuClient
{
    public async Task<Bar> GetBar()
    {
        var client = clientFactory.CreateRequestClient<GetBar>();
        var response = await client.GetResponse<Bar>(new {});
        return response.Message;
    }
}
