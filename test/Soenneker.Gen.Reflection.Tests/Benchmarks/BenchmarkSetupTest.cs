using Xunit;

namespace Soenneker.Gen.Reflection.Tests.Benchmarks;

public class BenchmarkSetupTest
{
    [Fact]
    public void BenchmarkSetup_ShouldWork()
    {
        // Simple test to verify the benchmarking setup works
        var person = new Person { Name = "Test", Age = 30 };
        TypeInfoGen typeInfo = person.GetTypeGen();
        
        Assert.NotNull(typeInfo);
        Assert.Equal("Person", typeInfo.Name);
        Assert.True(typeInfo.Properties.Length > 0);
    }
}
