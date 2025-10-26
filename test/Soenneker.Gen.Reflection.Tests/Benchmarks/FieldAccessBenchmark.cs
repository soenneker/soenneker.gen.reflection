using System;
using BenchmarkDotNet.Attributes;
using System.Reflection;

namespace Soenneker.Gen.Reflection.Tests.Benchmarks;

[MemoryDiagnoser]
[SimpleJob]
public class FieldAccessBenchmark
{
    private Person _person;
    private Company _company;
    private TypeInfoGen _personTypeInfo;
    private TypeInfoGen _companyTypeInfo;
    private Type _personType;
    private Type _companyType;

    [GlobalSetup]
    public void Setup()
    {
        _person = new Person { Name = "John Doe", Age = 30 };
        _company = new Company { Name = "Acme Corp", EmployeeCount = 100 };
        
        _personTypeInfo = _person.GetTypeGen();
        _companyTypeInfo = _company.GetTypeGen();
        
        _personType = _person.GetType();
        _companyType = _company.GetType();
    }

    [Benchmark(Baseline = true)]
    public FieldInfoGen? GetField_GenReflection()
    {
        return _personTypeInfo.GetField("_id");
    }

    [Benchmark]
    public FieldInfo? GetField_SystemReflection()
    {
        return _personType.GetField("_id", BindingFlags.NonPublic | BindingFlags.Instance);
    }

    [Benchmark]
    public FieldInfoGen[] GetAllFields_GenReflection()
    {
        return _personTypeInfo.Fields;
    }

    [Benchmark]
    public FieldInfo[] GetAllFields_SystemReflection()
    {
        return _personType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
    }

    [Benchmark]
    public object? GetFieldValue_GenReflection()
    {
        FieldInfoGen? field = _personTypeInfo.GetField("_id");
        return field?.GetValue(_person);
    }

    [Benchmark]
    public object? GetFieldValue_SystemReflection()
    {
        FieldInfo? field = _personType.GetField("_id", BindingFlags.NonPublic | BindingFlags.Instance);
        return field?.GetValue(_person);
    }
}
