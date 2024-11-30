namespace MiddlewareBenchmark.Consumers;

public record GenericConsumerState
{
    public GenericConsumerState()
    {
        this.Executed = false;
    }

    public bool Executed { get; set; }
}
