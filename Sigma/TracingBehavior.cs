using System.Diagnostics;
using MediatR;

namespace Sigma;

public class TracingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private static readonly ActivitySource ActivitySource = new("Sigma");

    public Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity(typeof(TRequest).FullName!);
        activity?.SetTag("RequestType", typeof(TRequest));
        activity?.SetTag("ResponseType", typeof(TResponse));
        return next();
    }
}
