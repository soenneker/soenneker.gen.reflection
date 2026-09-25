using System;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Soenneker.Gen.Reflection;

BenchmarkSwitcher.FromAssembly(typeof(PropertyBenchmarks).Assembly).Run(args);

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class PropertyBenchmarks
{
    private readonly BenchmarkModel _model = new() { P31 = 42 };
    private TypeInfoGen _metadata;
    private PropertyInfoGen _property;
    private PropertyInfo _runtimeProperty = null!;
    private Func<BenchmarkModel, int> _getter = null!;
    private Func<BenchmarkModel, int> _runtimeGetter = null!;
    private string _name = null!;
    private readonly Type _type = typeof(BenchmarkModel);

    [GlobalSetup]
    public void Setup()
    {
        _metadata = _model.GetTypeGen();
        _name = new string("P31".ToCharArray());
        _property = _metadata.GetProperty(_name)!.Value;
        _runtimeProperty = _type.GetProperty(_name)!;
        _getter = _property.GetGetter<BenchmarkModel, int>();
        _runtimeGetter = _runtimeProperty.GetMethod!.CreateDelegate<Func<BenchmarkModel, int>>();
    }

    [Benchmark(Baseline = true), BenchmarkCategory("Read")]
    public object? ReflectionRead() => _runtimeProperty.GetValue(_model);
    [Benchmark, BenchmarkCategory("Read")]
    public object? GeneratedObjectRead() => _property.GetValue(_model);
    [Benchmark, BenchmarkCategory("Read")]
    public int GeneratedTypedRead() => _getter(_model);
    [Benchmark, BenchmarkCategory("Read")]
    public int ReflectionDelegateRead() => _runtimeGetter(_model);
    [Benchmark(Baseline = true), BenchmarkCategory("LookupAndRead")]
    public object? ReflectionLookupAndRead() => _type.GetProperty(_name)!.GetValue(_model);
    [Benchmark, BenchmarkCategory("LookupAndRead")]
    public object? GeneratedLookupAndRead() => _metadata.GetProperty(_name)!.Value.GetValue(_model);
    [Benchmark(Baseline = true), BenchmarkCategory("Missing")]
    public PropertyInfo? ReflectionMissing() => _type.GetProperty("Missing");
    [Benchmark, BenchmarkCategory("Missing")]
    public PropertyInfoGen? GeneratedMissing() => _metadata.GetProperty("Missing");
}

public class BenchmarkModel
{
    public int P0 { get; set; }
    public int P1 { get; set; }
    public int P2 { get; set; }
    public int P3 { get; set; }
    public int P4 { get; set; }
    public int P5 { get; set; }
    public int P6 { get; set; }
    public int P7 { get; set; }
    public int P8 { get; set; }
    public int P9 { get; set; }
    public int P10 { get; set; }
    public int P11 { get; set; }
    public int P12 { get; set; }
    public int P13 { get; set; }
    public int P14 { get; set; }
    public int P15 { get; set; }
    public int P16 { get; set; }
    public int P17 { get; set; }
    public int P18 { get; set; }
    public int P19 { get; set; }
    public int P20 { get; set; }
    public int P21 { get; set; }
    public int P22 { get; set; }
    public int P23 { get; set; }
    public int P24 { get; set; }
    public int P25 { get; set; }
    public int P26 { get; set; }
    public int P27 { get; set; }
    public int P28 { get; set; }
    public int P29 { get; set; }
    public int P30 { get; set; }
    public int P31 { get; set; }
}
