using MassTransit;

namespace Mu.Client;

public class MuClient : IMuClient
{
    private readonly IClientFactory _clientFactory;

    public MuClient(IClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task<Bar> GetBar()
    {
        var client = _clientFactory.CreateRequestClient<GetBar>();
        var response = await client.GetResponse<Bar>(new {});
        return response.Message;
    }
}