using MongoDB.Driver;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

const string serviceName = "Omicron";
const string serviceVersion = "1.0.0";

var builder = WebApplication.CreateBuilder(args);
var (_, services, configuration, _, _, _) = builder;

builder.UseSerilog();

var connectionString = configuration.GetConnectionString(serviceName);

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

app.UseForwardedPathBase();

app.UseSwagger();
app.UseSwaggerUI();

await app.RunAsync();