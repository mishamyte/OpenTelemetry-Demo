using System.Reflection;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Sigma.Commands;
using Sigma.Persistence;
using Sigma.Queries;

const string serviceName = "Sigma";
const string serviceVersion = "1.0.0";

var builder = WebApplication.CreateBuilder(args);

builder.Host
    .ConfigureLogging(loggingBuilder => loggingBuilder.ClearProviders())
    .UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

var services = builder.Services;

var connectionString = builder.Configuration.GetConnectionString(serviceName);

services.AddDbContext<SigmaContext>(options => options.UseNpgsql(connectionString));

services.AddMediatR(Assembly.GetExecutingAssembly());

services.AddEndpointsApiExplorer()
    .AddSwaggerGen();

services.AddOpenTelemetryTracing(providerBuilder =>
{
    providerBuilder
        .AddSource(serviceName)
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName, serviceVersion: serviceVersion))
        .AddAspNetCoreInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddNpgsql()
        .AddOtlpExporter(options => options.Endpoint = new Uri("http://localhost:4317"));
});

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SigmaContext>();
    await context.Database.MigrateAsync();
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/user/list", async (IMediator mediator) =>
{
    var entities = await mediator.Send(new GetAllUsersQuery());
    return Results.Ok(entities);
});

app.MapPost("/user", async (CreateUserCommand request, IMediator mediator) => await mediator.Send(request));

await app.RunAsync();