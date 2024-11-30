namespace MiddlewareBenchmark.Interceptors;

public class NoopInterceptor : IConsumerInterceptor
{
    public Task OnHandle(Func<IConsumerContext, CancellationToken, Task> next, IConsumerContext context, CancellationToken cancellationToken)
    {
        return next(context, cancellationToken);
    }
}
