using Alpha;
using Epsilon.Client;
using MassTransit;
using Microsoft.Extensions.Options;
using Mu.Client;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Refit;
using Shared.Extensions;
using Shared.MassTransit;

const string serviceName = "Alpha";
const string serviceVersion = "1.0.0";

var builder = WebApplication.CreateBuilder(args);
var (_, services, configuration, _, _, _) = builder;

builder.UseSerilog();

services.AddEndpointsApiExplorer()
    .AddSwaggerGen();

services
    .AddRefitClient<IEpsilonClient>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(configuration["EpsilonUri"]));

// MassTransit over RabbitMq
services.Configure<MassTransitOptions>(configuration.GetSection(nameof(MassTransitOptions)));

services.AddMassTransit(
    configurator =>
    {
        configurator.SetKebabCaseEndpointNameFormatter();

        configurator.UsingRabbitMq(
            (ctx, cfg) =>
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

services.AddOpenTelemetryTracing(
    providerBuilder =>
    {
        providerBuilder
            .AddSource(serviceName)
            .AddSource("MassTransit")
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName, serviceVersion: serviceVersion))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter();
    });

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/aggregate", async (IEpsilonClient epsilonClient, IMuClient muClient) =>
{
    var foo = await epsilonClient.GetFoo();
    var bar = await muClient.GetBar();

    var aggregate = new Aggregate
    {
        FooId = foo.Id,
        FooName = foo.Name,
        BarId = bar.Id,
        BarCost = bar.Cost
    };

    return Results.Ok(aggregate);
});

await app.RunAsync();