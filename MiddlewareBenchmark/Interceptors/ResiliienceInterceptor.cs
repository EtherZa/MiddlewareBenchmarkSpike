namespace MiddlewareBenchmark.Interceptors;

public class ResiliienceInterceptor : IConsumerInterceptor
{
    public async Task OnHandle(Func<IConsumerContext, CancellationToken, Task> next, IConsumerContext context, CancellationToken cancellationToken)
    {
        // repeat 2 times; stimulating down stream re-scoping
        for (var i = 0; i < 2; i++)
        {
            try
            {
                await next(context, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
