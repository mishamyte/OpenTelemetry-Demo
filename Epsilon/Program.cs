using Elastic.Clients.Elasticsearch;
using Epsilon;
using Epsilon.Client;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Swashbuckle.AspNetCore.Annotations;

const string serviceName = "Epsilon";
const string serviceVersion = "1.0.0";
const string indexName = "epsilon";

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

var clientSettings = new ElasticsearchClientSettings(new Uri(configuration["Elasticsearch:Uri"]!))
    .DefaultIndex(indexName);
services.AddSingleton(new ElasticsearchClient(clientSettings));

services.AddOpenTelemetry().WithTracing(providerBuilder =>
    {
        providerBuilder
            .AddSource(serviceName)
            .AddSource("Elastic.Transport")
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName, serviceVersion: serviceVersion))
            .AddAspNetCoreInstrumentation()
            .AddOtlpExporter();
    })
    .WithMetrics(providerBuilder =>
    {
        providerBuilder
            .AddMeter(serviceName)
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName, serviceVersion: serviceVersion))
            .AddAspNetCoreInstrumentation()
            .AddRuntimeInstrumentation()
            .AddOtlpExporter();
    });

var app = builder.Build();

await ClearAndSeedIndex(app.Services);

app.UseForwardedPathBase();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet(
        "/foo",
        async (ElasticsearchClient elasticClient) =>
        {
            var entities = await elasticClient
                .SearchAsync<Foo>(s => s.Query(q => q.MatchAll()));
            var entity = entities.Documents.First();

            return Results.Ok(new FooDto(entity.Id, entity.Name));
        })
    .Produces<FooDto>()
    .WithMetadata(new SwaggerOperationAttribute("Try to find foo in ElasticSearch"));

await app.RunAsync();
return;

static async Task ClearAndSeedIndex(IServiceProvider serviceProvider)
{
    var elasticClient = serviceProvider.GetRequiredService<ElasticsearchClient>();

    var existsResponse = await elasticClient.Indices.ExistsAsync(indexName);
    if (existsResponse.Exists)
    {
        await elasticClient.Indices.DeleteAsync(indexName);
    }

    await elasticClient.Indices.CreateAsync(indexName);

    var foo = new Foo(Guid.Parse("575B0344-5EA6-4EC9-9186-0FAACC275484"), "Foo");

    await elasticClient.IndexAsync(foo);
}
