namespace MiddlewareBenchmark;

using BenchmarkDotNet.Attributes;

[MemoryDiagnoser]
public class MiddlewareBenchmark
{
    private readonly SampleMessage _message = new("Hello world");
    private PipelineRunner? _pipelineRunner;
    private IServiceProvider? _serviceProvider;

    [IterationSetup(Target = nameof(ScopedPipeline))]
    public void SetupScopedPipeline()
    {
        var serviceCollection = new ServiceCollection
        {
            ServiceDescriptor.Singleton(typeof(ResiliienceInterceptor), typeof(ResiliienceInterceptor)),
            ServiceDescriptor.Singleton(typeof(ServiceScopeInterceptor), typeof(ServiceScopeInterceptor)),
            ServiceDescriptor.Singleton(typeof(TransactionInterceptor), typeof(TransactionInterceptor)),
            ServiceDescriptor.Singleton(typeof(ConsumerHost), typeof(ConsumerHost)),
            ServiceDescriptor.Transient(typeof(IConsumer<>), typeof(GenericConsumer<>)),
            ServiceDescriptor.Scoped<GenericConsumerState, GenericConsumerState>()
        };

        this._serviceProvider = serviceCollection.BuildServiceProvider();

        // prime host
        var _ = this._serviceProvider.GetRequiredService<ConsumerHost>().ConsumerDelegate(typeof(SampleMessage));

        this._pipelineRunner = new PipelineRunner(this._serviceProvider);

        this._pipelineRunner.AddInterceptor<ResiliienceInterceptor>();
        this._pipelineRunner.AddInterceptor<ServiceScopeInterceptor>();
        this._pipelineRunner.AddInterceptor<TransactionInterceptor>();
        this._pipelineRunner.AddInterceptor<ConsumerHost>();
    }

    [Benchmark]
    public async Task ScopedPipeline()
    {
        await this._pipelineRunner!.ExecuteAsync(this._message, CancellationToken.None);
    }

    [IterationSetup(Target = nameof(NoScopePipeline))]
    public void SetupNoScopePipeline()
    {
        var serviceCollection = new ServiceCollection
        {
            ServiceDescriptor.Singleton(typeof(TransactionInterceptor), typeof(TransactionInterceptor)),
            ServiceDescriptor.Singleton(typeof(ConsumerHost), typeof(ConsumerHost)),
            ServiceDescriptor.Transient(typeof(IConsumer<>), typeof(GenericConsumer<>)),
            ServiceDescriptor.Scoped<GenericConsumerState, GenericConsumerState>()
        };

        this._serviceProvider = serviceCollection.BuildServiceProvider();

        // prime host
        var _ = this._serviceProvider.GetRequiredService<ConsumerHost>().ConsumerDelegate(typeof(SampleMessage));

        this._pipelineRunner = new PipelineRunner(this._serviceProvider);

        this._pipelineRunner.AddInterceptor<TransactionInterceptor>();
        this._pipelineRunner.AddInterceptor<ConsumerHost>();
    }

    [Benchmark]
    public async Task NoScopePipeline()
    {
        await this._pipelineRunner!.ExecuteAsync(this._message, CancellationToken.None);
    }

    [IterationSetup(Target = nameof(OneThousandInterationNoopPipeline))]
    public void SetupOneThousandInterationNoopPipeline()
    {
        var serviceCollection = new ServiceCollection
        {
            ServiceDescriptor.Singleton(typeof(NoopInterceptor), typeof(NoopInterceptor)),
            ServiceDescriptor.Singleton(typeof(ConsumerHost), typeof(ConsumerHost)),
            ServiceDescriptor.Transient(typeof(IConsumer<>), typeof(GenericConsumer<>)),
            ServiceDescriptor.Scoped<GenericConsumerState, GenericConsumerState>()
        };

        this._serviceProvider = serviceCollection.BuildServiceProvider();

        // prime host
        var _ = this._serviceProvider.GetRequiredService<ConsumerHost>().ConsumerDelegate(typeof(SampleMessage));

        this._pipelineRunner = new PipelineRunner(this._serviceProvider);

        for (var i = 0; i < 1000; i++)
        {
            this._pipelineRunner.AddInterceptor<NoopInterceptor>();
        }

        this._pipelineRunner.AddInterceptor<ConsumerHost>();
    }

    [Benchmark]
    public async Task OneThousandInterationNoopPipeline()
    {
        await this._pipelineRunner!.ExecuteAsync(this._message, CancellationToken.None);
    }

    [IterationSetup(Target = nameof(MinPipeline))]
    public void SetupMinPipeline()
    {
        var serviceCollection = new ServiceCollection
        {
            ServiceDescriptor.Singleton(typeof(ServiceScopeInterceptor), typeof(ServiceScopeInterceptor)),
            ServiceDescriptor.Singleton(typeof(ConsumerHost), typeof(ConsumerHost)),
            ServiceDescriptor.Transient(typeof(IConsumer<>), typeof(GenericConsumer<>)),
            ServiceDescriptor.Scoped<GenericConsumerState, GenericConsumerState>()
        };

        this._serviceProvider = serviceCollection.BuildServiceProvider();

        // prime host
        var _ = this._serviceProvider.GetRequiredService<ConsumerHost>().ConsumerDelegate(typeof(SampleMessage));

        this._pipelineRunner = new PipelineRunner(this._serviceProvider);
        this._pipelineRunner.AddInterceptor<ServiceScopeInterceptor>();
        this._pipelineRunner.AddInterceptor<ConsumerHost>();
    }

    [Benchmark]
    public async Task MinPipeline()
    {
        await this._pipelineRunner!.ExecuteAsync(this._message, CancellationToken.None);
    }

    [IterationSetup(Target = nameof(NoPipeline))]
    public void SetupNoPipeline()
    {
        var serviceCollection = new ServiceCollection
        {
            ServiceDescriptor.Singleton(typeof(ConsumerHost), typeof(ConsumerHost)),
            ServiceDescriptor.Transient(typeof(IConsumer<>), typeof(GenericConsumer<>)),
            ServiceDescriptor.Scoped<GenericConsumerState, GenericConsumerState>()
        };

        this._serviceProvider = serviceCollection.BuildServiceProvider();

        // prime host
        var _ = this._serviceProvider.GetRequiredService<ConsumerHost>().ConsumerDelegate(typeof(SampleMessage));
    }

    [Benchmark]
    public async Task NoPipeline()
    {
        var message = new SampleMessage("Hello world");

        await using var scope = this._serviceProvider!.CreateAsyncScope();
        var context = new ConsumerContext(message.GetType(), message);
        context.Features.Add(typeof(IServiceProvider), this._serviceProvider!);

        var consumerHost = scope.ServiceProvider.GetRequiredService<ConsumerHost>();
        await consumerHost.OnHandle(null!, context, CancellationToken.None);
    }

    public record SampleMessage(string Message);
}
