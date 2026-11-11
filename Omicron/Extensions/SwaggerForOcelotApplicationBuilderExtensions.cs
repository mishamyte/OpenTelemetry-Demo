using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class SwaggerForOcelotApplicationBuilderExtensions
{
    private const string DocsBasePath = "/swagger/docs";

    extension(IApplicationBuilder app)
    {
        /// <summary>
        /// Short-circuits unknown <c>/swagger/docs/{version}/{key}</c> requests with a 404.
        /// </summary>
        /// <remarks>
        /// SwaggerForOcelotMiddleware looks the requested version/key pair straight up in a
        /// dictionary, so an unconfigured pair (or a missing segment) escapes as a
        /// KeyNotFoundException and surfaces as an unhandled 500. Its lookup is case-sensitive
        /// even though the base path match is not, so the comparison below mirrors both.
        /// </remarks>
        public IApplicationBuilder UseSwaggerForOcelotDocsGuard(IConfiguration configuration)
        {
            var knownDocs = configuration.GetSection("SwaggerEndPoints")
                .GetChildren()
                .SelectMany(endPoint => endPoint
                    .GetSection("Config")
                    .GetChildren()
                    .Select(config => new PathString($"/{config["Version"]}/{endPoint["Key"]}")))
                .ToList();

            app.Use((context, next) =>
            {
                if (context.Request.Path.StartsWithSegments(DocsBasePath, out var requestedDoc)
                    && !knownDocs.Any(known => requestedDoc.StartsWithSegments(known, StringComparison.Ordinal)))
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return Task.CompletedTask;
                }

                return next();
            });

            return app;
        }
    }
}
