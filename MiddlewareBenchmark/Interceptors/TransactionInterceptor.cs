namespace MiddlewareBenchmark.Interceptors;

using System.Transactions;

public class TransactionInterceptor : IConsumerInterceptor
{
    public async Task OnHandle(Func<IConsumerContext, CancellationToken, Task> next, IConsumerContext context, CancellationToken cancellationToken)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Suppress);
        await next(context, cancellationToken);
        scope.Complete();
    }
}
