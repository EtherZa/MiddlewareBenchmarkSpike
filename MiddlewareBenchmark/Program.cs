namespace MiddlewareBenchmark;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

internal class Program
{
    static void Main(string[] args)
    {
#if DEBUG
        BenchmarkRunner.Run<MiddlewareBenchmark>(new DebugInProcessConfig(), args);
#else
        BenchmarkRunner.Run<MiddlewareBenchmark>(args: args);
#endif
    }
}