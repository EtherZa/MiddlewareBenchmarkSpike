namespace MiddlewareBenchmark.Contracts;

public interface IConsumer<in TMessage>
{
    Task OnHandle(TMessage message, CancellationToken cancellationToken);
}
