namespace MiddlewareBenchmark.Consumers;

public class GenericConsumer<TMessage> : IConsumer<TMessage>
{
    private readonly GenericConsumerState _consumerState;
    public GenericConsumer(GenericConsumerState consumerState)
    {
        this._consumerState = consumerState;
    }

    public Task OnHandle(TMessage message, CancellationToken cancellationToken)
    {
        if (this._consumerState.Executed)
        {
            throw new Exception("Scoped object has been shared again");
        }

        this._consumerState.Executed = true;

        return Task.CompletedTask;
    }
}
