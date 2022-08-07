using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Shared.Extensions;

const string serviceName = "Omicron";
const string serviceVersion = "1.0.0";

var builder = WebApplication.CreateBuilder(args);
var (environment, services, configuration, _, _, _) = builder;

builder.UseSerilog();

builder.Configuration
    .AddJsonFile("ocelot.json", false, true)
    .AddJsonFile($"ocelot.{environment.EnvironmentName}.json", true, true);

services.AddOcelot();

services.AddOpenTelemetryTracing(providerBuilder =>
{
    providerBuilder
        .AddSource(serviceName)
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName, serviceVersion: serviceVersion))
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter();
});

var app = builder.Build();

await app.UseOcelot();

await app.RunAsync();