using Epsilon;
using Epsilon.Client;
using Nest;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

const string serviceName = "Epsilon";
const string serviceVersion = "1.0.0";
const string indexName = "epsilon";

var builder = WebApplication.CreateBuilder(args);
var (_, services, configuration, _, _, _) = builder;

builder.UseSerilog();

services.AddEndpointsApiExplorer()
    .AddSwaggerGen();

var connectionSettings = new ConnectionSettings(new Uri(configuration["Elasticsearch:Uri"]))
    .DefaultIndex(indexName);
services.AddSingleton<IElasticClient>(_ => new ElasticClient(connectionSettings));

services.AddOpenTelemetryTracing(
    providerBuilder =>
    {
        providerBuilder
            .AddSource(serviceName)
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName, serviceVersion: serviceVersion))
            .AddAspNetCoreInstrumentation()
            .AddElasticsearchClientInstrumentation()
            .AddOtlpExporter();
    });

var app = builder.Build();

ClearAndSeedIndex(app.Services);

app.UseForwardedPathBase();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/foo", async (IElasticClient elasticClient) =>
{
    var entities = await elasticClient
        .SearchAsync<Foo>(s => s.Query(q => q.MatchAll()));
    var entity = entities.Documents.First();

    return Results.Ok(new FooDto(entity.Id, entity.Name));
});

await app.RunAsync();

static void ClearAndSeedIndex(IServiceProvider serviceProvider)
{
    var elasticClient = serviceProvider.GetRequiredService<IElasticClient>();

    if (elasticClient.Indices.Exists(indexName).Exists)
    {
        elasticClient.Indices.Delete(indexName);
    }

    elasticClient.Indices.Create(
        indexName,
        descriptor => descriptor.Map<Foo>(m => m.AutoMap()));

    var foo = new Foo(Guid.Parse("575B0344-5EA6-4EC9-9186-0FAACC275484"), "Foo");

    elasticClient.IndexDocument(foo);
}