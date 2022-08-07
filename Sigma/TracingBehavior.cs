using System.Diagnostics;
using MediatR;

namespace Sigma;

public class TracingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public Task<TResponse> Handle(
        TRequest request,
        CancellationToken cancellationToken,
        RequestHandlerDelegate<TResponse> next)
    {
        using var activity = new ActivitySource("Sigma").StartActivity(typeof(TRequest).FullName!);
        activity?.SetTag("RequestType", typeof(TRequest));
        activity?.SetTag("ResponseType", typeof(TResponse));
        return next();
    }
}