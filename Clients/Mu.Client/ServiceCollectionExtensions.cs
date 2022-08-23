using Microsoft.Extensions.DependencyInjection;

namespace Mu.Client;

public static class ServiceCollectionExtensions
{
    public static void AddMuClient(this IServiceCollection services) => services.AddTransient<IMuClient, MuClient>();
}