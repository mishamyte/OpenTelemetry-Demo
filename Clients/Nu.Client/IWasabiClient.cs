using Refit;

namespace Nu.Client;

public interface IWasabiClient
{
    [Get("/wasabi")]
    Task<WasabiDto> GetWasabi();
}