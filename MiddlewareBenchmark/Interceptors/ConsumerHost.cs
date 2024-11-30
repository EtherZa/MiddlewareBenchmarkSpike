namespace MiddlewareBenchmark.Interceptors;

using System.Collections.Concurrent;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;

public class ConsumerHost : IConsumerInterceptor
{
    private static readonly ConcurrentDictionary<Type, (Type ConsumerType, Func<object, object, CancellationToken, Task> Delegate)> _handleMethods = new();
    private static readonly object _serviceProviderKey = typeof(IServiceProvider);

    public async Task OnHandle(Func<IConsumerContext, CancellationToken, Task> next, IConsumerContext context, CancellationToken cancellationToken)
    {
        if (!context.Features.TryGetValue(_serviceProviderKey, out var rawServiceProvider) || rawServiceProvider is not IServiceProvider serviceProvider)
        {
            throw new ApplicationException("Context does not contain the service provider");
        }

        var data = ConsumerDelegate(context.MessageType);
        var consumerInstance = serviceProvider.GetRequiredService(data.ConsumerType);
        try
        {
            await data.Delegate.Invoke(consumerInstance, context.Message, cancellationToken);
        }
        finally
        {
            if (consumerInstance is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }
            else
            {
                (consumerInstance as IDisposable)?.Dispose();
            }
        }
    }

    public (Type ConsumerType, Func<object, object, CancellationToken, Task> Delegate) ConsumerDelegate(Type messageType)
    {
        if (messageType == null)
        {
            throw new ArgumentNullException(nameof(messageType));
        }

        var data = _handleMethods.GetOrAdd(
            messageType,
            static type =>
            {
                var consumerType = typeof(IConsumer<>).MakeGenericType(type);
                var methodInfo = consumerType.GetMethod(nameof(IConsumer<object>.OnHandle));

                if (methodInfo == null)
                {
                    throw new InvalidOperationException("OnHandle method not found.");
                }

                // Create a parameter expression for the consumer instance and the message
                var consumerParam = Expression.Parameter(typeof(object), "consumer");
                var messageParam = Expression.Parameter(typeof(object), "message");
                var cancellationTokenParam = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

                // Convert the parameters to the correct types
                var typedConsumer = Expression.Convert(consumerParam, consumerType);
                var typedMessage = Expression.Convert(messageParam, type);

                // Create a call to the OnHandle method
                var callExpr = Expression.Call(typedConsumer, methodInfo, typedMessage, cancellationTokenParam);

                // Compile the expression into a delegate
                var lambda = Expression.Lambda<Func<object, object, CancellationToken, Task>>(callExpr, consumerParam, messageParam, cancellationTokenParam);
                var x = lambda.Compile();

                return (consumerType, x);
            });

        return data;
    }
}
