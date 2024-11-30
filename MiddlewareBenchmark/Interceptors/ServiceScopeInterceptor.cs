namespace MiddlewareBenchmark.Interceptors;

public class ServiceScopeInterceptor : IConsumerInterceptor
{
    private static readonly object _serviceProviderKey = typeof(IServiceProvider);

    public async Task OnHandle(Func<IConsumerContext, CancellationToken, Task> next, IConsumerContext context, CancellationToken cancellationToken)
    {
        if (!context.Features.TryGetValue(_serviceProviderKey, out var rawServiceProvider) || rawServiceProvider is not IServiceProvider serviceProvider)
        {
            throw new ApplicationException("Context does not contain the service provider");
        }

        try
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            context.Features[_serviceProviderKey] = scope.ServiceProvider;
            await next(context, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            context.Features[_serviceProviderKey] = serviceProvider;
        }
    }
}
