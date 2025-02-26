using MediatR;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Sigma;
using Sigma.Commands;
using Sigma.Persistence;
using Sigma.Queries;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;

const string serviceName = "Sigma";
const string serviceVersion = "1.0.0";

var builder = WebApplication.CreateBuilder(args);
var (_, services, configuration, _, _, _) = builder;

builder.UseSerilog();

var connectionString = configuration.GetConnectionString(serviceName);

services.AddDbContext<SigmaContext>(options => options.UseNpgsql(connectionString));

// Explicit creation of multiplexer
// For RedisCache it is passed by factory below
// For Redis instrumentation it is injected by DI (or could be passed explicitly)
IConnectionMultiplexer redisConnectionMultiplexer =
    await ConnectionMultiplexer.ConnectAsync(configuration.GetConnectionString("Redis")!);
services.AddSingleton(redisConnectionMultiplexer);
services.AddStackExchangeRedisCache(
    options =>
        options.ConnectionMultiplexerFactory = () => Task.FromResult(redisConnectionMultiplexer));

// MediatR + Tracing Behavior for it's handlers
services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TracingBehavior<,>));

services.AddEndpointsApiExplorer()
    .AddSwaggerGen(
        options =>
        {
            options.CustomSchemaIds(type => type.FullName!.Replace('+', '.'));
            options.DescribeAllParametersInCamelCase();
            options.EnableAnnotations();
        });

services.AddOpenTelemetry()
    .WithTracing(
        providerBuilder =>
        {
            providerBuilder
                .AddSource(serviceName)
                .SetResourceBuilder(
                    ResourceBuilder.CreateDefault().AddService(serviceName, serviceVersion: serviceVersion))
                .AddAspNetCoreInstrumentation()
                .AddEntityFrameworkCoreInstrumentation()
                .AddNpgsql()
                .AddRedisInstrumentation()
                .AddOtlpExporter();
        }).WithMetrics(
        providerBuilder =>
        {
            providerBuilder
                .AddMeter(serviceName)
                .SetResourceBuilder(
                    ResourceBuilder.CreateDefault()
                        .AddService(serviceName, serviceVersion: serviceVersion))
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation()
                .AddOtlpExporter();
        });

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SigmaContext>();
    await context.Database.MigrateAsync();
}

app.UseForwardedPathBase();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet(
        "/user/{id:guid}",
        async (Guid id, IMediator mediator) =>
        {
            var entity = await mediator.Send(new GetUserByIdQuery { Id = id });
            return entity != null ? Results.Ok(entity) : Results.NotFound();
        })
    .Produces<GetUserByIdQuery.User>()
    .Produces(StatusCodes.Status404NotFound)
    .WithMetadata(
        new SwaggerOperationAttribute("Try to find user in Redis, otherwise try to find it in Posgres with EF Core"));

app.MapGet(
        "/user/list",
        async (IMediator mediator) =>
        {
            var entities = await mediator.Send(new GetAllUsersQuery());
            return Results.Ok(entities);
        })
    .Produces<IEnumerable<GetAllUsersQuery.User>>()
    .WithMetadata(new SwaggerOperationAttribute("Get all users from Postgres via Dapper"));

app.MapPost("/user", async (CreateUserCommand request, IMediator mediator) => await mediator.Send(request))
    .Produces(StatusCodes.Status200OK)
    .WithMetadata(new SwaggerOperationAttribute("Create user in Postgres with EF Core"));

await app.RunAsync();