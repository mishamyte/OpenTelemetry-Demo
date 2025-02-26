using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;
using Nu;
using Nu.Client;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Swashbuckle.AspNetCore.Annotations;

const string serviceName = "Nu";
const string serviceVersion = "1.0.0";

var builder = WebApplication.CreateBuilder(args);
var (_, services, configuration, _, _, _) = builder;

builder.UseSerilog();

var connectionString = configuration.GetConnectionString(serviceName);

BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

services.AddSingleton<IMongoClient, MongoClient>(
    _ =>
    {
        var settings = MongoClientSettings.FromConnectionString(connectionString);
        settings.ClusterConfigurator =
            clusterBuilder => clusterBuilder.Subscribe(new DiagnosticsActivityEventSubscriber());
        return new MongoClient(settings);
    });

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
                .AddMongoDBInstrumentation()
                .AddOtlpExporter();
        })
    .WithMetrics(
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

await ClearAndSeedCollection(app.Services);

app.UseForwardedPathBase();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet(
        "/wasabi",
        async (IMongoClient mongoClient) =>
        {
            var db = mongoClient.GetDatabase(serviceName);
            var collection = db.GetCollection<Wasabi>(nameof(Wasabi));
            var entity = await collection.Find(_ => true).FirstOrDefaultAsync();
            return entity != null ? Results.Ok(new WasabiDto(entity.Id, entity.Name)) : Results.NotFound();
        })
    .Produces<WasabiDto>()
    .Produces(StatusCodes.Status404NotFound)
    .WithMetadata(new SwaggerOperationAttribute("Try to find wasabi in Mongo"));

await app.RunAsync();
return;

static async Task ClearAndSeedCollection(IServiceProvider serviceProvider)
{
    var client = serviceProvider.GetRequiredService<IMongoClient>();
    var db = client.GetDatabase(serviceName);
    await db.DropCollectionAsync(nameof(Wasabi));
    var collection = db.GetCollection<Wasabi>(nameof(Wasabi));
    await collection.InsertOneAsync(new Wasabi(Guid.Parse("4179BBFB-0222-4932-95AA-1CA780ACB79A"), "Wasabi"));
}