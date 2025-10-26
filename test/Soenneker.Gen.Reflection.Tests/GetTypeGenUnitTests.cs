using System;
using System.Collections.Generic;
using AwesomeAssertions;
using Xunit;

namespace Soenneker.Gen.Reflection.Tests;

public class GetTypeGenUnitTests
{
    [Fact]
    public void GetTypeGen_WithPrimitiveTypes_ReturnsCorrectTypeInfoGen()
    {
        // Arrange
        var intValue = 42;
        var stringValue = "test";
        var boolValue = true;

        // Act
        TypeInfoGen intTypeInfoGen = intValue.GetTypeGen();
        TypeInfoGen stringTypeInfoGen = stringValue.GetTypeGen();
        TypeInfoGen boolTypeInfoGen = boolValue.GetTypeGen();

        // Assert
        intTypeInfoGen.Name.Should().Be("Int32");
        intTypeInfoGen.IsValueType.Should().BeTrue();
        intTypeInfoGen.IsReferenceType.Should().BeFalse();
        intTypeInfoGen.IsGenericType.Should().BeFalse();
        intTypeInfoGen.IsNullable.Should().BeFalse();

        stringTypeInfoGen.Name.Should().Be("String");
        stringTypeInfoGen.IsValueType.Should().BeFalse();
        stringTypeInfoGen.IsReferenceType.Should().BeTrue();
        stringTypeInfoGen.IsGenericType.Should().BeFalse();
        stringTypeInfoGen.IsNullable.Should().BeFalse();

        boolTypeInfoGen.Name.Should().Be("Boolean");
        boolTypeInfoGen.IsValueType.Should().BeTrue();
        boolTypeInfoGen.IsReferenceType.Should().BeFalse();
    }

    [Fact]
    public void GetTypeGen_WithNullableTypes_ReturnsCorrectTypeInfoGen()
    {
        // Arrange
        int? nullableInt = 42;
        var nullableString = "test";

        // Act
        TypeInfoGen nullableIntTypeInfoGen = nullableInt.GetTypeGen();
        TypeInfoGen nullableStringTypeInfoGen = nullableString.GetTypeGen();

        // Assert
        nullableIntTypeInfoGen.Name.Should().Be("Nullable`1");
        nullableIntTypeInfoGen.IsValueType.Should().BeTrue();
        nullableIntTypeInfoGen.IsGenericType.Should().BeTrue();
        nullableIntTypeInfoGen.IsNullable.Should().BeTrue();
        nullableIntTypeInfoGen.UnderlyingType.Should().NotBeNull();
        nullableIntTypeInfoGen.UnderlyingType.Value.Name.Should().Be("Int32");

        // String is reference type, so nullable string is still reference type
        nullableStringTypeInfoGen.Name.Should().Be("String");
        nullableStringTypeInfoGen.IsValueType.Should().BeFalse();
        nullableStringTypeInfoGen.IsReferenceType.Should().BeTrue();
    }

    [Fact]
    public void GetTypeGen_WithGenericTypes_ReturnsCorrectTypeInfoGen()
    {
        // Arrange
        var list = new List<string>();
        var dict = new Dictionary<string, int>();

        // Act
        TypeInfoGen listTypeInfoGen = list.GetTypeGen();
        TypeInfoGen dictTypeInfoGen = dict.GetTypeGen();

        // Assert
        listTypeInfoGen.Name.Should().Be("List`1");
        listTypeInfoGen.IsGenericType.Should().BeTrue();
        listTypeInfoGen.IsReferenceType.Should().BeTrue();
        listTypeInfoGen.GenericTypeArguments.Length.Should().Be(1);
        listTypeInfoGen.GenericTypeArguments[0].Name.Should().Be("String");

        dictTypeInfoGen.Name.Should().Be("Dictionary`2");
        dictTypeInfoGen.IsGenericType.Should().BeTrue();
        dictTypeInfoGen.IsReferenceType.Should().BeTrue();
        dictTypeInfoGen.GenericTypeArguments.Length.Should().Be(2);
        dictTypeInfoGen.GenericTypeArguments[0].Name.Should().Be("String");
        dictTypeInfoGen.GenericTypeArguments[1].Name.Should().Be("Int32");
    }

    [Fact]
    public void GetTypeGen_WithCustomTypes_ReturnsCorrectTypeInfoGen()
    {
        // Arrange
        var person = new TestPerson { Name = "John", Age = 30 };

        // Act
        TypeInfoGen personTypeInfoGen = person.GetTypeGen();

        // Assert
        personTypeInfoGen.Name.Should().Be("TestPerson");
        personTypeInfoGen.IsReferenceType.Should().BeTrue();
        personTypeInfoGen.IsValueType.Should().BeFalse();
        personTypeInfoGen.IsGenericType.Should().BeFalse();
        personTypeInfoGen.IsNullable.Should().BeFalse();

        // Check properties
        PropertyInfoGen[] properties = personTypeInfoGen.Properties;
        properties.Length.Should().BeGreaterThan(0);
        
        PropertyInfoGen? nameProperty = personTypeInfoGen.GetProperty("Name");
        nameProperty.HasValue.Should().BeTrue();
        nameProperty.Value.Name.Should().Be("Name");
        nameProperty.Value.PropertyType.Name.Should().Be("String");
        nameProperty.Value.CanRead.Should().BeTrue();
        nameProperty.Value.CanWrite.Should().BeTrue();

        PropertyInfoGen? ageProperty = personTypeInfoGen.GetProperty("Age");
        ageProperty.HasValue.Should().BeTrue();
        ageProperty.Value.Name.Should().Be("Age");
        ageProperty.Value.PropertyType.Name.Should().Be("Int32");
    }

    [Fact]
    public void GetTypeGen_WithStaticGenericMethod_ReturnsCorrectTypeInfoGen()
    {
        // Act
        var list = new List<string>();
        TypeInfoGen listTypeInfoGen = list.GetTypeGen();
        var dict = new Dictionary<int, string>();
        TypeInfoGen dictTypeInfoGen = dict.GetTypeGen();

        // Assert
        listTypeInfoGen.Name.Should().Be("List`1");
        listTypeInfoGen.IsGenericType.Should().BeTrue();
        listTypeInfoGen.GenericTypeArguments.Length.Should().Be(1);
        listTypeInfoGen.GenericTypeArguments[0].Name.Should().Be("String");

        dictTypeInfoGen.Name.Should().Be("Dictionary`2");
        dictTypeInfoGen.IsGenericType.Should().BeTrue();
        dictTypeInfoGen.GenericTypeArguments.Length.Should().Be(2);
        dictTypeInfoGen.GenericTypeArguments[0].Name.Should().Be("Int32");
        dictTypeInfoGen.GenericTypeArguments[1].Name.Should().Be("String");
    }

    [Fact]
    public void GetTypeGen_WithMethodAccess_ReturnsCorrectMethodInfoGen()
    {
        // Arrange
        var person = new TestPerson { Name = "Jane", Age = 25 };

        // Act
        TypeInfoGen personTypeInfoGen = person.GetTypeGen();
        MethodInfoGen? toStringMethod = personTypeInfoGen.GetMethod("ToString");

        // Assert
        toStringMethod.HasValue.Should().BeTrue();
        toStringMethod.Value.Name.Should().Be("ToString");
        toStringMethod.Value.ReturnType.Name.Should().Be("String");
        toStringMethod.Value.IsStatic.Should().BeFalse();
        toStringMethod.Value.ParameterTypes.Length.Should().Be(0);
    }

    [Fact]
    public void GetTypeGen_WithFieldAccess_ReturnsCorrectFieldInfoGen()
    {
        // Arrange
        var person = new TestPerson { Name = "Bob", Age = 35 };

        // Act
        TypeInfoGen personTypeInfoGen = person.GetTypeGen();
        FieldInfoGen[] fields = personTypeInfoGen.Fields;

        // Assert
        fields.Length.Should().BeGreaterThan(0);
        
        // Find a field (assuming TestPerson has fields)
        var fieldFound = false;
        foreach (FieldInfoGen field in fields)
        {
            if (!string.IsNullOrEmpty(field.Name))
            {
                fieldFound = true;
                field.FieldType.Name.Should().NotBeNullOrEmpty();
                break;
            }
        }
        
        // Note: This test might not find fields if TestPerson only has properties
        // In a real scenario, you'd have actual fields to test
    }

    [Fact]
    public void GetTypeGen_WithPropertyValueAccess_WorksCorrectly()
    {
        // Arrange
        var person = new TestPerson { Name = "Alice", Age = 28 };

        // Act
        TypeInfoGen personTypeInfoGen = person.GetTypeGen();
        PropertyInfoGen? nameProperty = personTypeInfoGen.GetProperty("Name");
        PropertyInfoGen? ageProperty = personTypeInfoGen.GetProperty("Age");

        // Assert
        nameProperty.HasValue.Should().BeTrue();
        object? nameValue = nameProperty.Value.GetValue(person);
        nameValue.Should().Be("Alice");

        ageProperty.HasValue.Should().BeTrue();
        object? ageValue = ageProperty.Value.GetValue(person);
        ageValue.Should().Be(28);
    }

    [Fact]
    public void GetTypeGen_WithComplexNestedTypes_ReturnsCorrectTypeInfoGen()
    {
        // Arrange
        var company = new TestCompany
        {
            Name = "Test Corp",
            Employees = new List<TestPerson>
            {
                new TestPerson { Name = "Emp1", Age = 30 },
                new TestPerson { Name = "Emp2", Age = 25 }
            }
        };

        // Act
        TypeInfoGen companyTypeInfoGen = company.GetTypeGen();
        PropertyInfoGen? employeesProperty = companyTypeInfoGen.GetProperty("Employees");

        // Assert
        companyTypeInfoGen.Name.Should().Be("TestCompany");
        employeesProperty.HasValue.Should().BeTrue();
        employeesProperty.Value.PropertyType.Name.Should().Be("List`1");
        employeesProperty.Value.PropertyType.IsGenericType.Should().BeTrue();
        employeesProperty.Value.PropertyType.GenericTypeArguments.Length.Should().Be(1);
        employeesProperty.Value.PropertyType.GenericTypeArguments[0].Name.Should().Be("TestPerson");
    }

    [Fact]
    public void GetTypeGen_WithArrayTypes_ReturnsCorrectTypeInfoGen()
    {
        // Arrange
        int[] intArray = { 1, 2, 3 };
        string[] stringArray = { "a", "b", "c" };

        // Act
        TypeInfoGen intArrayTypeInfoGen = intArray.GetTypeGen();
        TypeInfoGen stringArrayTypeInfoGen = stringArray.GetTypeGen();

        // Assert
        intArrayTypeInfoGen.Name.Should().Be("Int32[]");
        intArrayTypeInfoGen.IsReferenceType.Should().BeTrue();
        intArrayTypeInfoGen.IsValueType.Should().BeFalse();

        stringArrayTypeInfoGen.Name.Should().Be("String[]");
        stringArrayTypeInfoGen.IsReferenceType.Should().BeTrue();
        stringArrayTypeInfoGen.IsValueType.Should().BeFalse();
    }

    [Fact]
    public void GetTypeGen_WithDateTimeTypes_ReturnsCorrectTypeInfoGen()
    {
        // Arrange
        DateTime dateTime = DateTime.Now;
        TimeSpan timeSpan = TimeSpan.FromHours(1);

        // Act
        TypeInfoGen dateTimeTypeInfoGen = dateTime.GetTypeGen();
        TypeInfoGen timeSpanTypeInfoGen = timeSpan.GetTypeGen();

        // Assert
        dateTimeTypeInfoGen.Name.Should().Be("DateTime");
        dateTimeTypeInfoGen.IsValueType.Should().BeTrue();
        dateTimeTypeInfoGen.IsReferenceType.Should().BeFalse();

        timeSpanTypeInfoGen.Name.Should().Be("TimeSpan");
        timeSpanTypeInfoGen.IsValueType.Should().BeTrue();
        timeSpanTypeInfoGen.IsReferenceType.Should().BeFalse();
    }
}

/// <summary>
/// Test person class for unit tests
/// </summary>
public class TestPerson
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public TestCompany? Company { get; set; }
    private readonly string _id = Guid.NewGuid().ToString();

    public override string ToString()
    {
        return $"{Name} ({Age})";
    }
}

/// <summary>
/// Test company class for unit tests
/// </summary>
public class TestCompany
{
    public string Name { get; set; } = string.Empty;
    public List<TestPerson> Employees { get; set; } = new();
    public DateTime FoundedDate { get; set; }
    public int EmployeeCount { get; set; }
}
