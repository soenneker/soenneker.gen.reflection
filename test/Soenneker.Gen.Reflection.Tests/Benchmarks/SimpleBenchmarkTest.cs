using System;
using BenchmarkDotNet.Attributes;
using System.Reflection;

namespace Soenneker.Gen.Reflection.Tests.Benchmarks;

[MemoryDiagnoser]
[SimpleJob]
public class SimpleBenchmarkTest
{
    private Person _person;
    private TypeInfoGen _personTypeInfo;
    private Type _personType;

    [GlobalSetup]
    public void Setup()
    {
        _person = new Person { Name = "John Doe", Age = 30 };
        _personTypeInfo = _person.GetTypeGen();
        _personType = _person.GetType();
    }

    [Benchmark(Baseline = true)]
    public TypeInfoGen GetTypeGen()
    {
        return _person.GetTypeGen();
    }

    [Benchmark]
    public Type GetTypeReflection()
    {
        return _person.GetType();
    }

    [Benchmark]
    public PropertyInfoGen? GetProperty_GenReflection()
    {
        return _personTypeInfo.GetProperty("Name");
    }

    [Benchmark]
    public PropertyInfo? GetProperty_SystemReflection()
    {
        return _personType.GetProperty("Name");
    }
}
