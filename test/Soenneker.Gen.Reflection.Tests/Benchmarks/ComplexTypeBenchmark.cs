using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using System.Reflection;

namespace Soenneker.Gen.Reflection.Tests.Benchmarks;

[MemoryDiagnoser]
[SimpleJob]
public class ComplexTypeBenchmark
{
    private TestPerson _testPerson;
    private TestCompany _testCompany;
    private TypeInfoGen _testPersonTypeInfo;
    private TypeInfoGen _testCompanyTypeInfo;
    private Type _testPersonType;
    private Type _testCompanyType;

    [GlobalSetup]
    public void Setup()
    {
        _testPerson = new TestPerson 
        { 
            Name = "John Doe", 
            Age = 30,
            Company = new TestCompany { Name = "Acme Corp", EmployeeCount = 100 }
        };
        
        _testCompany = new TestCompany 
        { 
            Name = "Tech Inc", 
            EmployeeCount = 50,
            Employees = new List<TestPerson>
            {
                new TestPerson { Name = "Alice", Age = 25 },
                new TestPerson { Name = "Bob", Age = 35 }
            }
        };
        
        _testPersonTypeInfo = _testPerson.GetTypeGen();
        _testCompanyTypeInfo = _testCompany.GetTypeGen();
        
        _testPersonType = _testPerson.GetType();
        _testCompanyType = _testCompany.GetType();
    }

    [Benchmark(Baseline = true)]
    public TypeInfoGen GetTypeInfo_GenReflection()
    {
        return _testPerson.GetTypeGen();
    }

    [Benchmark]
    public Type GetTypeInfo_SystemReflection()
    {
        return _testPerson.GetType();
    }

    [Benchmark]
    public PropertyInfoGen? GetNestedProperty_GenReflection()
    {
        PropertyInfoGen? companyProperty = _testPersonTypeInfo.GetProperty("Company");
        if (companyProperty.HasValue && companyProperty.Value.PropertyType.Properties != null)
        {
            return companyProperty.Value.PropertyType.GetProperty("Name");
        }
        return null;
    }

    [Benchmark]
    public PropertyInfo? GetNestedProperty_SystemReflection()
    {
        PropertyInfo? companyProperty = _testPersonType.GetProperty("Company");
        if (companyProperty?.PropertyType != null)
        {
            return companyProperty.PropertyType.GetProperty("Name");
        }
        return null;
    }

    [Benchmark]
    public PropertyInfoGen? GetGenericProperty_GenReflection()
    {
        return _testCompanyTypeInfo.GetProperty("Employees");
    }

    [Benchmark]
    public PropertyInfo? GetGenericProperty_SystemReflection()
    {
        return _testCompanyType.GetProperty("Employees");
    }

    [Benchmark]
    public TypeInfoGen? GetUnderlyingType_GenReflection()
    {
        PropertyInfoGen? nullableProperty = _testPersonTypeInfo.GetProperty("Age");
        return nullableProperty?.PropertyType.UnderlyingType;
    }

    [Benchmark]
    public Type? GetUnderlyingType_SystemReflection()
    {
        PropertyInfo? nullableProperty = _testPersonType.GetProperty("Age");
        return Nullable.GetUnderlyingType(nullableProperty?.PropertyType);
    }

    [Benchmark]
    public TypeInfoGen[] GetGenericArguments_GenReflection()
    {
        PropertyInfoGen? employeesProperty = _testCompanyTypeInfo.GetProperty("Employees");
        return employeesProperty?.PropertyType.GenericTypeArguments ?? Array.Empty<TypeInfoGen>();
    }

    [Benchmark]
    public Type[] GetGenericArguments_SystemReflection()
    {
        PropertyInfo? employeesProperty = _testCompanyType.GetProperty("Employees");
        return employeesProperty?.PropertyType.GetGenericArguments() ?? Array.Empty<Type>();
    }
}
