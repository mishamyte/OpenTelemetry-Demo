using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Shared.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static void Deconstruct(
        this WebApplicationBuilder builder,
        out IWebHostEnvironment environment,
        out IServiceCollection services,
        out ConfigurationManager configuration,
        out ILoggingBuilder logging,
        out ConfigureWebHostBuilder webHost,
        out ConfigureHostBuilder host)
    {
        environment = builder.Environment;
        services = builder.Services;
        configuration = builder.Configuration;
        logging = builder.Logging;
        webHost = builder.WebHost;
        host = builder.Host;
    }

    public static void UseSerilog(this WebApplicationBuilder builder)
    {
        builder.Host
            .ConfigureLogging(loggingBuilder => loggingBuilder.ClearProviders())
            .UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));
    }
}