using Refit;

namespace Epsilon.Client;

public interface IEpsilonClient
{
    [Get("/foo")]
    Task<FooDto> GetFoo();
}