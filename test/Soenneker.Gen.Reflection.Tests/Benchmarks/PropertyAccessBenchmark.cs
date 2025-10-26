using BenchmarkDotNet.Attributes;
using System;
using System.Reflection;

namespace Soenneker.Gen.Reflection.Tests.Benchmarks;

[MemoryDiagnoser]
[SimpleJob]
public class PropertyAccessBenchmark
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
    public PropertyInfoGen? GetProperty_GenReflection_Name()
    {
        return _personTypeInfo.GetProperty("Name");
    }

    [Benchmark]
    public PropertyInfo? GetProperty_SystemReflection_Name()
    {
        return _personType.GetProperty("Name");
    }

    [Benchmark]
    public PropertyInfoGen? GetProperty_GenReflection_Age()
    {
        return _personTypeInfo.GetProperty("Age");
    }

    [Benchmark]
    public PropertyInfo? GetProperty_SystemReflection_Age()
    {
        return _personType.GetProperty("Age");
    }

    [Benchmark]
    public PropertyInfoGen? GetProperty_GenReflection_CompanyName()
    {
        return _companyTypeInfo.GetProperty("Name");
    }

    [Benchmark]
    public PropertyInfo? GetProperty_SystemReflection_CompanyName()
    {
        return _companyType.GetProperty("Name");
    }

    [Benchmark]
    public PropertyInfoGen? GetProperty_GenReflection_EmployeeCount()
    {
        return _companyTypeInfo.GetProperty("EmployeeCount");
    }

    [Benchmark]
    public PropertyInfo? GetProperty_SystemReflection_EmployeeCount()
    {
        return _companyType.GetProperty("EmployeeCount");
    }

    [Benchmark]
    public PropertyInfoGen[] GetAllProperties_GenReflection()
    {
        return _personTypeInfo.Properties;
    }

    [Benchmark]
    public PropertyInfo[] GetAllProperties_SystemReflection()
    {
        return _personType.GetProperties();
    }
}
