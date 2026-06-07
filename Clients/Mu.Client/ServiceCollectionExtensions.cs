using Microsoft.Extensions.DependencyInjection;

namespace Mu.Client;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddMuClient()
        {
            services.AddTransient<IMuClient, MuClient>();
        }
    }
}