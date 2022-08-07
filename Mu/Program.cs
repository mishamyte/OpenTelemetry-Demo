using MassTransit;
using Microsoft.Extensions.Options;
using Mu;
using Mu.Dtos;
using Mu.Extensions;
using Mu.Messages;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Shared.Extensions;

const string serviceName = "Mu";
const string serviceVersion = "1.0.0";

var builder = WebApplication.CreateBuilder(args);
var (_, services, configuration, _, _, _) = builder;

builder.UseSerilog();

services.AddEndpointsApiExplorer()
    .AddSwaggerGen();

// MassTransit over RabbitMq
services.Configure<MassTransitOptions>(configuration.GetSection(nameof(MassTransitOptions)));

services.AddMassTransit(configurator =>
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

    configurator.AddConsumers(typeof(Program).Assembly);
});

services.AddOpenTelemetryTracing(providerBuilder =>
{
    providerBuilder
        .AddSource(serviceName)
        .AddSource("MassTransit")
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName, serviceVersion: serviceVersion))
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter();
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost(
    "/command",
    async (CommandDto request, ISendEndpointProvider provider, CancellationToken cancellationToken) =>
    {
        var endpoint = await provider.GetSendEndpoint<Command>();
        await endpoint.Send<Command>(
            new
            {
                Payload = request.Payload
            },
            cancellationToken);

        return Results.Ok();
    });

app.MapPost(
    "/publish",
    async (PublishDto request, IPublishEndpoint publishEndpoint, CancellationToken cancellationToken) =>
    {
        await publishEndpoint.Publish<Publish>(
            new
            {
                Payload = request.Payload
            },
            cancellationToken);

        return Results.Ok();
    });

await app.RunAsync();