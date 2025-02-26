using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseForwardedPathBase(this IApplicationBuilder app)
    {
        app.Use(
            (context, next) =>
            {
                if (context.Request.Headers.TryGetValue("X-Forwarded-PathBase", out var pathsBase))
                {
                    context.Request.PathBase = new PathString(pathsBase);
                }

                return next();
            });

        return app;
    }
}