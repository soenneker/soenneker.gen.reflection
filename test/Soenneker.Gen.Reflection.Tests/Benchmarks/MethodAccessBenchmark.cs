using System;
using BenchmarkDotNet.Attributes;
using System.Reflection;

namespace Soenneker.Gen.Reflection.Tests.Benchmarks;

[MemoryDiagnoser]
[SimpleJob]
public class MethodAccessBenchmark
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
    public MethodInfoGen? GetMethod_GenReflection()
    {
        return _personTypeInfo.GetMethod("ToString");
    }

    [Benchmark]
    public MethodInfo? GetMethod_SystemReflection()
    {
        return _personType.GetMethod("ToString");
    }

    [Benchmark]
    public MethodInfoGen[] GetAllMethods_GenReflection()
    {
        return _personTypeInfo.Methods;
    }

    [Benchmark]
    public MethodInfo[] GetAllMethods_SystemReflection()
    {
        return _personType.GetMethods();
    }

    [Benchmark]
    public MethodInfoGen? GetMethodWithParameters_GenReflection()
    {
        return _personTypeInfo.GetMethod("ToString");
    }

    [Benchmark]
    public MethodInfo? GetMethodWithParameters_SystemReflection()
    {
        return _personType.GetMethod("ToString", Type.EmptyTypes);
    }
}
