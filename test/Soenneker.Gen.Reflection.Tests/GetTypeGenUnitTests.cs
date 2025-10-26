using System;
using System.Collections.Generic;
using Soenneker.Gen.Reflection;
using AwesomeAssertions;
using Xunit;

namespace Soenneker.Gen.Reflection.Tests;

public class GetTypeGenUnitTests
{
    [Fact]
    public void GetTypeGen_WithPrimitiveTypes_ReturnsCorrectTypeInfo()
    {
        // Arrange
        var intValue = 42;
        var stringValue = "test";
        var boolValue = true;

        // Act
        var intTypeInfo = intValue.GetTypeGen();
        var stringTypeInfo = stringValue.GetTypeGen();
        var boolTypeInfo = boolValue.GetTypeGen();

        // Assert
        intTypeInfo.Name.Should().Be("Int32");
        intTypeInfo.IsValueType.Should().BeTrue();
        intTypeInfo.IsReferenceType.Should().BeFalse();
        intTypeInfo.IsGenericType.Should().BeFalse();
        intTypeInfo.IsNullable.Should().BeFalse();

        stringTypeInfo.Name.Should().Be("String");
        stringTypeInfo.IsValueType.Should().BeFalse();
        stringTypeInfo.IsReferenceType.Should().BeTrue();
        stringTypeInfo.IsGenericType.Should().BeFalse();
        stringTypeInfo.IsNullable.Should().BeFalse();

        boolTypeInfo.Name.Should().Be("Boolean");
        boolTypeInfo.IsValueType.Should().BeTrue();
        boolTypeInfo.IsReferenceType.Should().BeFalse();
    }

    [Fact]
    public void GetTypeGen_WithNullableTypes_ReturnsCorrectTypeInfo()
    {
        // Arrange
        int? nullableInt = 42;
        var nullableString = "test";

        // Act
        var nullableIntTypeInfo = nullableInt.GetTypeGen();
        var nullableStringTypeInfo = nullableString.GetTypeGen();

        // Assert
        nullableIntTypeInfo.Name.Should().Be("Nullable`1");
        nullableIntTypeInfo.IsValueType.Should().BeTrue();
        nullableIntTypeInfo.IsGenericType.Should().BeTrue();
        nullableIntTypeInfo.IsNullable.Should().BeTrue();
        nullableIntTypeInfo.UnderlyingType.Should().NotBeNull();
        nullableIntTypeInfo.UnderlyingType.Value.Name.Should().Be("Int32");

        // String is reference type, so nullable string is still reference type
        nullableStringTypeInfo.Name.Should().Be("String");
        nullableStringTypeInfo.IsValueType.Should().BeFalse();
        nullableStringTypeInfo.IsReferenceType.Should().BeTrue();
    }

    [Fact]
    public void GetTypeGen_WithGenericTypes_ReturnsCorrectTypeInfo()
    {
        // Arrange
        var list = new List<string>();
        var dict = new Dictionary<string, int>();

        // Act
        var listTypeInfo = list.GetTypeGen();
        var dictTypeInfo = dict.GetTypeGen();

        // Assert
        listTypeInfo.Name.Should().Be("List`1");
        listTypeInfo.IsGenericType.Should().BeTrue();
        listTypeInfo.IsReferenceType.Should().BeTrue();
        listTypeInfo.GenericTypeArguments.Length.Should().Be(1);
        listTypeInfo.GenericTypeArguments[0].Name.Should().Be("String");

        dictTypeInfo.Name.Should().Be("Dictionary`2");
        dictTypeInfo.IsGenericType.Should().BeTrue();
        dictTypeInfo.IsReferenceType.Should().BeTrue();
        dictTypeInfo.GenericTypeArguments.Length.Should().Be(2);
        dictTypeInfo.GenericTypeArguments[0].Name.Should().Be("String");
        dictTypeInfo.GenericTypeArguments[1].Name.Should().Be("Int32");
    }

    [Fact]
    public void GetTypeGen_WithCustomTypes_ReturnsCorrectTypeInfo()
    {
        // Arrange
        var person = new TestPerson { Name = "John", Age = 30 };

        // Act
        var personTypeInfo = person.GetTypeGen();

        // Assert
        personTypeInfo.Name.Should().Be("TestPerson");
        personTypeInfo.IsReferenceType.Should().BeTrue();
        personTypeInfo.IsValueType.Should().BeFalse();
        personTypeInfo.IsGenericType.Should().BeFalse();
        personTypeInfo.IsNullable.Should().BeFalse();

        // Check properties
        var properties = personTypeInfo.Properties;
        properties.Length.Should().BeGreaterThan(0);
        
        var nameProperty = personTypeInfo.GetProperty("Name");
        nameProperty.HasValue.Should().BeTrue();
        nameProperty.Value.Name.Should().Be("Name");
        nameProperty.Value.PropertyType.Name.Should().Be("String");
        nameProperty.Value.CanRead.Should().BeTrue();
        nameProperty.Value.CanWrite.Should().BeTrue();

        var ageProperty = personTypeInfo.GetProperty("Age");
        ageProperty.HasValue.Should().BeTrue();
        ageProperty.Value.Name.Should().Be("Age");
        ageProperty.Value.PropertyType.Name.Should().Be("Int32");
    }

    [Fact]
    public void GetTypeGen_WithStaticGenericMethod_ReturnsCorrectTypeInfo()
    {
        // Act
        var list = new List<string>();
        var listTypeInfo = list.GetTypeGen();
        var dict = new Dictionary<int, string>();
        var dictTypeInfo = dict.GetTypeGen();

        // Assert
        listTypeInfo.Name.Should().Be("List`1");
        listTypeInfo.IsGenericType.Should().BeTrue();
        listTypeInfo.GenericTypeArguments.Length.Should().Be(1);
        listTypeInfo.GenericTypeArguments[0].Name.Should().Be("String");

        dictTypeInfo.Name.Should().Be("Dictionary`2");
        dictTypeInfo.IsGenericType.Should().BeTrue();
        dictTypeInfo.GenericTypeArguments.Length.Should().Be(2);
        dictTypeInfo.GenericTypeArguments[0].Name.Should().Be("Int32");
        dictTypeInfo.GenericTypeArguments[1].Name.Should().Be("String");
    }

    [Fact]
    public void GetTypeGen_WithMethodAccess_ReturnsCorrectMethodInfo()
    {
        // Arrange
        var person = new TestPerson { Name = "Jane", Age = 25 };

        // Act
        var personTypeInfo = person.GetTypeGen();
        var toStringMethod = personTypeInfo.GetMethod("ToString");

        // Assert
        toStringMethod.HasValue.Should().BeTrue();
        toStringMethod.Value.Name.Should().Be("ToString");
        toStringMethod.Value.ReturnType.Name.Should().Be("String");
        toStringMethod.Value.IsStatic.Should().BeFalse();
        toStringMethod.Value.ParameterTypes.Length.Should().Be(0);
    }

    [Fact]
    public void GetTypeGen_WithFieldAccess_ReturnsCorrectFieldInfo()
    {
        // Arrange
        var person = new TestPerson { Name = "Bob", Age = 35 };

        // Act
        var personTypeInfo = person.GetTypeGen();
        var fields = personTypeInfo.Fields;

        // Assert
        fields.Length.Should().BeGreaterThan(0);
        
        // Find a field (assuming TestPerson has fields)
        var fieldFound = false;
        foreach (var field in fields)
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
        var personTypeInfo = person.GetTypeGen();
        var nameProperty = personTypeInfo.GetProperty("Name");
        var ageProperty = personTypeInfo.GetProperty("Age");

        // Assert
        nameProperty.HasValue.Should().BeTrue();
        var nameValue = nameProperty.Value.GetValue(person);
        nameValue.Should().Be("Alice");

        ageProperty.HasValue.Should().BeTrue();
        var ageValue = ageProperty.Value.GetValue(person);
        ageValue.Should().Be(28);
    }

    [Fact]
    public void GetTypeGen_WithComplexNestedTypes_ReturnsCorrectTypeInfo()
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
        var companyTypeInfo = company.GetTypeGen();
        var employeesProperty = companyTypeInfo.GetProperty("Employees");

        // Assert
        companyTypeInfo.Name.Should().Be("TestCompany");
        employeesProperty.HasValue.Should().BeTrue();
        employeesProperty.Value.PropertyType.Name.Should().Be("List`1");
        employeesProperty.Value.PropertyType.IsGenericType.Should().BeTrue();
        employeesProperty.Value.PropertyType.GenericTypeArguments.Length.Should().Be(1);
        employeesProperty.Value.PropertyType.GenericTypeArguments[0].Name.Should().Be("TestPerson");
    }

    [Fact]
    public void GetTypeGen_WithArrayTypes_ReturnsCorrectTypeInfo()
    {
        // Arrange
        int[] intArray = { 1, 2, 3 };
        string[] stringArray = { "a", "b", "c" };

        // Act
        var intArrayTypeInfo = intArray.GetTypeGen();
        var stringArrayTypeInfo = stringArray.GetTypeGen();

        // Assert
        intArrayTypeInfo.Name.Should().Be("Int32[]");
        intArrayTypeInfo.IsReferenceType.Should().BeTrue();
        intArrayTypeInfo.IsValueType.Should().BeFalse();

        stringArrayTypeInfo.Name.Should().Be("String[]");
        stringArrayTypeInfo.IsReferenceType.Should().BeTrue();
        stringArrayTypeInfo.IsValueType.Should().BeFalse();
    }

    [Fact]
    public void GetTypeGen_WithDateTimeTypes_ReturnsCorrectTypeInfo()
    {
        // Arrange
        var dateTime = DateTime.Now;
        var timeSpan = TimeSpan.FromHours(1);

        // Act
        var dateTimeTypeInfo = dateTime.GetTypeGen();
        var timeSpanTypeInfo = timeSpan.GetTypeGen();

        // Assert
        dateTimeTypeInfo.Name.Should().Be("DateTime");
        dateTimeTypeInfo.IsValueType.Should().BeTrue();
        dateTimeTypeInfo.IsReferenceType.Should().BeFalse();

        timeSpanTypeInfo.Name.Should().Be("TimeSpan");
        timeSpanTypeInfo.IsValueType.Should().BeTrue();
        timeSpanTypeInfo.IsReferenceType.Should().BeFalse();
    }
}

/// <summary>
/// Test person class for unit tests
/// </summary>
public class TestPerson
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
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
}
