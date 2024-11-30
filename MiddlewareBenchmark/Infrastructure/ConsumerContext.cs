namespace MiddlewareBenchmark.Infrastructure;

public record ConsumerContext : IConsumerContext
{
    public ConsumerContext(Type messageType, object message)
    {
        this.Features = new Dictionary<object, object>();
        this.Message = message;
        this.MessageType = messageType;
    }

    public IDictionary<object, object> Features { get; private set; }

    public object Message { get; private set; }

    public Type MessageType { get; private set; }
}
