using Alpha;
using Epsilon.Client;
using MassTransit;
using Microsoft.Extensions.Options;
using Mu.Client;
using Nu.Client;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Refit;
using Shared.MassTransit;
using Swashbuckle.AspNetCore.Annotations;

const string serviceName = "Alpha";
const string serviceVersion = "1.0.0";

var builder = WebApplication.CreateBuilder(args);
var (_, services, configuration, _, _, _) = builder;

builder.UseSerilog();

services.AddEndpointsApiExplorer()
    .AddSwaggerGen(options =>
    {
        options.CustomSchemaIds(type => type.FullName!.Replace('+', '.'));
        options.DescribeAllParametersInCamelCase();
        options.EnableAnnotations();
    });

// Refit Http Clients
services
    .AddRefitClient<IEpsilonClient>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(configuration["EpsilonUri"]!));

services
    .AddRefitClient<INuClient>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(configuration["NuUri"]!));

// MassTransit over RabbitMq
services.Configure<MassTransitOptions>(configuration.GetSection(nameof(MassTransitOptions)));

services.AddMassTransit(configurator =>
{
    configurator.SetKebabCaseEndpointNameFormatter();

    configurator.UsingRabbitMq((ctx, cfg) =>
    {
        var options = ctx.GetRequiredService<IOptions<MassTransitOptions>>().Value;

        cfg.Host(
            options.Host,
            host =>
            {
                host.Username(options.Username);
                host.Password(options.Password);
            });

        cfg.ConfigureEndpoints(ctx);
    });
});

services.AddMuClient();

services.AddOpenTelemetry()
    .WithTracing(providerBuilder =>
    {
        providerBuilder
            .AddSource(serviceName)
            .AddSource("MassTransit")
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName, serviceVersion: serviceVersion))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter();
    }).WithMetrics(providerBuilder =>
    {
        providerBuilder
            .AddMeter(serviceName)
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName, serviceVersion: serviceVersion))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddOtlpExporter();
    });

var app = builder.Build();

app.UseForwardedPathBase();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet(
        "/aggregate",
        async (
            IEpsilonClient epsilonClient,
            IMuClient muClient,
            INuClient nuClient) =>
        {
            var foo = await epsilonClient.GetFoo();
            var bar = await muClient.GetBar();
            var wasabi = await nuClient.GetWasabi();

            var aggregate = new Aggregate
            {
                FooId = foo.Id,
                FooName = foo.Name,
                BarId = bar.Id,
                BarCost = bar.Cost,
                WasabiId = wasabi.Id,
                WasabiName = wasabi.Name
            };

            return Results.Ok(aggregate);
        })
    .Produces<Aggregate>()
    .WithMetadata(
        new SwaggerOperationAttribute(
            "Aggregate data from Epsilon & Nu via Refit and Mu via MassTransit's request/response"));

await app.RunAsync();