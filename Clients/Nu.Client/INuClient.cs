using Refit;

namespace Nu.Client;

public interface INuClient
{
    [Get("/wasabi")]
    Task<WasabiDto> GetWasabi();
}