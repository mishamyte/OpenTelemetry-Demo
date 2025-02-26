using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

const string serviceName = "Omicron";
const string serviceVersion = "1.0.0";

var builder = WebApplication.CreateBuilder(args);
var (environment, services, configuration, _, _, _) = builder;

builder.UseSerilog();

builder.Configuration
    .AddJsonFile(
        "ocelot.json",
        false,
        true)
    .AddJsonFile(
        $"ocelot.{environment.EnvironmentName}.json",
        true,
        true);

services.AddOcelot();

services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen();

services.AddSwaggerForOcelot(configuration);

services.AddOpenTelemetry()
    .WithTracing(
        providerBuilder =>
        {
            providerBuilder
                .AddSource(serviceName)
                .SetResourceBuilder(
                    ResourceBuilder.CreateDefault().AddService(serviceName, serviceVersion: serviceVersion))
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter();
        })
    .WithMetrics(
        providerBuilder =>
        {
            providerBuilder
                .AddMeter(serviceName)
                .SetResourceBuilder(
                    ResourceBuilder.CreateDefault().AddService(serviceName, serviceVersion: serviceVersion))
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddOtlpExporter();
        });

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerForOcelotUI();

await app.UseOcelot();

await app.RunAsync();