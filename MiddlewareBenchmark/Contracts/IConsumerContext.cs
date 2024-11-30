namespace MiddlewareBenchmark.Contracts;

public interface IConsumerContext
{
    IDictionary<object, object> Features { get; }

    object Message { get; }

    Type MessageType { get; }
}
