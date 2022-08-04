using System.Diagnostics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

const string serviceName = "Alpha";
const string serviceVersion = "1.0.0";

var builder = WebApplication.CreateBuilder(args);

builder.Host
    .ConfigureLogging(loggingBuilder => loggingBuilder.ClearProviders())
    .UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

var services = builder.Services;

services.AddOpenTelemetryTracing(providerBuilder =>
{
    providerBuilder
        .AddSource(serviceName)
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName, serviceVersion: serviceVersion))
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter(options => options.Endpoint = new Uri("http://localhost:4317"));
});

var app = builder.Build();

app.MapGet("/hello", () =>
{
    using var activity = new ActivitySource(serviceName).StartActivity("Root");
    activity?.SetTag("Foo", "Bar");
    activity?.SetTag("Alpha", new[] { "Beta", "Gamma" });

    return Results.Ok("Hello World!");
});

await app.RunAsync();