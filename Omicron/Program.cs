using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

const string serviceName = "Omicron";
const string serviceVersion = "1.0.0";

var builder = WebApplication.CreateBuilder(args);

builder.Host
    .ConfigureLogging(loggingBuilder => loggingBuilder.ClearProviders())
    .UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

builder.Configuration
    .AddJsonFile("ocelot.json", false, true)
    .AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", true, true);

var services = builder.Services;

services.AddOcelot();

services.AddOpenTelemetryTracing(providerBuilder =>
{
    providerBuilder
        .AddSource(serviceName)
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName, serviceVersion: serviceVersion))
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(options => options.Endpoint = new Uri("http://localhost:4317"));
});

var app = builder.Build();

await app.UseOcelot();

await app.RunAsync();