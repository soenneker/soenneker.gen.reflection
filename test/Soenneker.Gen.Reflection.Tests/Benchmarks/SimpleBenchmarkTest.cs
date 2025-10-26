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
    
    // Field to prevent JIT elision
    private object _result;

    [GlobalSetup]
    public void Setup()
    {
        _person = new Person { Name = "John Doe", Age = 30 };
        _personTypeInfo = _person.GetTypeGen();
        _personType = _person.GetType();
        
        // Warm up the types to avoid class constructor cost in benchmarks
        _ = _person.GetTypeGen(); // Warm up GetTypeGen
        _ = _person.GetType(); // Warm up GetType
        _ = _personTypeInfo.GetProperty("Name"); // Warm up property access
        _ = _personType.GetProperty("Name"); // Warm up reflection property access
    }

    [Benchmark(Baseline = true)]
    public TypeInfoGen GetTypeGen()
    {
        TypeInfoGen result = _person.GetTypeGen();
        _result = result; // Prevent JIT elision
        return result;
    }

    [Benchmark]
    public Type GetTypeReflection()
    {
        Type result = _person.GetType();
        _result = result; // Prevent JIT elision
        return result;
    }

    [Benchmark]
    public PropertyInfoGen? GetProperty_GenReflection()
    {
        PropertyInfoGen? result = _personTypeInfo.GetProperty("Name");
        _result = result; // Prevent JIT elision
        return result;
    }

    [Benchmark]
    public PropertyInfo? GetProperty_SystemReflection()
    {
        PropertyInfo? result = _personType.GetProperty("Name");
        _result = result; // Prevent JIT elision
        return result;
    }
}
