namespace MiddlewareBenchmark.Infrastructure;

public class PipelineRunner
{
    private static readonly object _stateKey = new();
    private static readonly object _serviceProviderKey = typeof(IServiceProvider);

    private readonly IServiceProvider _serviceProvider;
    private readonly IList<Type> _interceptors;

    public PipelineRunner(IServiceProvider serviceProvider)
    {
        this._serviceProvider = serviceProvider;
        this._interceptors = [];
    }

    public void AddInterceptor<T>()
        where T : IConsumerInterceptor
    {
        this._interceptors.Add(typeof(T));
    }

    public async Task ExecuteAsync(object message, CancellationToken cancellationToken)
    {
        var context = new ConsumerContext(message.GetType(), message);
        context.Features.Add(_stateKey, 0);
        context.Features.Add(_serviceProviderKey, this._serviceProvider);

        async Task NextAsync(IConsumerContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!context.Features.TryGetValue(_stateKey, out var rawState) || rawState is not int interceptorIndex)
            {
                throw new ApplicationException("State corrupted");
            }

            if (!context.Features.TryGetValue(typeof(IServiceProvider), out var rawServiceProvider) || rawServiceProvider is not IServiceProvider serviceProvider)
            {
                throw new ApplicationException($"{nameof(IServiceProvider)} is not available");
            }

            try
            {
                var interceptorType = this._interceptors[interceptorIndex];
                var interceptor = (IConsumerInterceptor)serviceProvider.GetRequiredService(interceptorType);
                context.Features[_stateKey] = ++interceptorIndex;
                try
                {
                    await interceptor.OnHandle((ctx, cancellationToken) => NextAsync(ctx, cancellationToken), context, cancellationToken).ConfigureAwait(false);
                }
                finally
                {
                    context.Features[_stateKey] = --interceptorIndex;
                    if (interceptor is IAsyncDisposable asyncDisposable)
                    {
                        await asyncDisposable.DisposeAsync();
                    }
                    else
                    {
                        (interceptor as IDisposable)?.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                // unhandled. do something
                throw;
            }
        }

        await NextAsync(context, cancellationToken);
    }
}
