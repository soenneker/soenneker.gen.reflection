using System;
using BenchmarkDotNet.Attributes;

namespace Soenneker.Gen.Reflection.Tests.Benchmarks;

[MemoryDiagnoser]
[SimpleJob]
public class TypeInfoAccessBenchmark
{
    private Person _person;
    private Company _company;
    private TestPerson _testPerson;
    private TestCompany _testCompany;

    [GlobalSetup]
    public void Setup()
    {
        _person = new Person { Name = "John Doe", Age = 30 };
        _company = new Company { Name = "Acme Corp", EmployeeCount = 100 };
        _testPerson = new TestPerson { Name = "Jane Smith", Age = 25 };
        _testCompany = new TestCompany { Name = "Tech Inc", EmployeeCount = 50 };
    }

    [Benchmark(Baseline = true)]
    public TypeInfoGen GetTypeGen_Person()
    {
        return _person.GetTypeGen();
    }

    [Benchmark]
    public Type GetReflection_Person()
    {
        return _person.GetType();
    }

    [Benchmark]
    public TypeInfoGen GetTypeGen_Company()
    {
        return _company.GetTypeGen();
    }

    [Benchmark]
    public Type GetReflection_Company()
    {
        return _company.GetType();
    }

    [Benchmark]
    public TypeInfoGen GetTypeGen_TestPerson()
    {
        return _testPerson.GetTypeGen();
    }

    [Benchmark]
    public Type GetReflection_TestPerson()
    {
        return _testPerson.GetType();
    }

    [Benchmark]
    public TypeInfoGen GetTypeGen_TestCompany()
    {
        return _testCompany.GetTypeGen();
    }

    [Benchmark]
    public Type GetReflection_TestCompany()
    {
        return _testCompany.GetType();
    }
}
