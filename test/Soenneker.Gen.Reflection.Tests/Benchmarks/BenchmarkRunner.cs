using BenchmarkDotNet.Reports;
using Soenneker.Benchmarking.Extensions.Summary;
using Soenneker.Tests.Benchmark;
using System.Threading.Tasks;
using Soenneker.Facts.Local;
using Xunit;

namespace Soenneker.Gen.Reflection.Tests.Benchmarks;

public class BenchmarkRunner : BenchmarkTest
{
    public BenchmarkRunner(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    //[LocalFact]
    public async ValueTask TypeInfoAccessBenchmark()
    {
        Summary summary = BenchmarkDotNet.Running.BenchmarkRunner.Run<TypeInfoAccessBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog(OutputHelper, CancellationToken);
    }

   // [LocalFact]
    public async ValueTask PropertyAccessBenchmark()
    {
        Summary summary = BenchmarkDotNet.Running.BenchmarkRunner.Run<PropertyAccessBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog(OutputHelper, CancellationToken);
    }

   // [LocalFact]
    public async ValueTask FieldAccessBenchmark()
    {
        Summary summary = BenchmarkDotNet.Running.BenchmarkRunner.Run<FieldAccessBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog(OutputHelper, CancellationToken);
    }

   // [LocalFact]
    public async ValueTask MethodAccessBenchmark()
    {
        Summary summary = BenchmarkDotNet.Running.BenchmarkRunner.Run<MethodAccessBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog(OutputHelper, CancellationToken);
    }

  //  [LocalFact]
    public async ValueTask ComplexTypeBenchmark()
    {
        Summary summary = BenchmarkDotNet.Running.BenchmarkRunner.Run<ComplexTypeBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog(OutputHelper, CancellationToken);
    }

   [LocalFact]
    public async ValueTask SimpleBenchmarkTest()
    {
        Summary summary = BenchmarkDotNet.Running.BenchmarkRunner.Run<SimpleBenchmarkTest>(DefaultConf);

        await summary.OutputSummaryToLog(OutputHelper, CancellationToken);
    }
}
