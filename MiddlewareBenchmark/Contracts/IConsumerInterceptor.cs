namespace MiddlewareBenchmark.Contracts;

public interface IConsumerInterceptor
{
    Task OnHandle(Func<IConsumerContext, CancellationToken, Task> next, IConsumerContext context, CancellationToken cancellationToken);
}
