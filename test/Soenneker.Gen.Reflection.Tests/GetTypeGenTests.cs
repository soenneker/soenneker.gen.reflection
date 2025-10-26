using System;
using System.Collections.Generic;
using Soenneker.Gen.Reflection;

namespace Soenneker.Gen.Reflection.Tests;

/// <summary>
/// Test class demonstrating GetTypeGen usage patterns
/// </summary>
public class GetTypeGenTests
{
    public void BasicUsageTests()
    {
        // Test with primitive types
        int intValue = 42;
        var intTypeInfo = intValue.GetTypeGen();
        Console.WriteLine($"Int type name: {intTypeInfo.Name}");
        Console.WriteLine($"Int is value type: {intTypeInfo.IsValueType}");

        // Test with string
        string stringValue = "Hello World";
        var stringTypeInfo = stringValue.GetTypeGen();
        Console.WriteLine($"String type name: {stringTypeInfo.Name}");
        Console.WriteLine($"String is reference type: {stringTypeInfo.IsReferenceType}");

        // Test with generic method call
        var list = new List<string>();
        var listTypeInfo = list.GetTypeGen();
        Console.WriteLine($"List<string> type name: {listTypeInfo.Name}");
        Console.WriteLine($"List<string> is generic: {listTypeInfo.IsGenericType}");
    }

    public void NullableTypeTests()
    {
        // Test with nullable types
        int? nullableInt = 42;
        var nullableTypeInfo = nullableInt.GetTypeGen();
        Console.WriteLine($"Nullable int type name: {nullableTypeInfo.Name}");
        Console.WriteLine($"Nullable int is nullable: {nullableTypeInfo.IsNullable}");
        
        if (nullableTypeInfo.UnderlyingType.HasValue)
        {
            Console.WriteLine($"Underlying type: {nullableTypeInfo.UnderlyingType.Value.Name}");
        }
    }

    public void GenericTypeTests()
    {
        // Test with generic types
        var dict = new Dictionary<string, int>();
        var dictTypeInfo = dict.GetTypeGen();
        Console.WriteLine($"Dictionary type name: {dictTypeInfo.Name}");
        Console.WriteLine($"Dictionary is generic: {dictTypeInfo.IsGenericType}");
        
        var genericArgs = dictTypeInfo.GenericTypeArguments;
        Console.WriteLine($"Generic arguments count: {genericArgs.Length}");
        foreach (var arg in genericArgs)
        {
            Console.WriteLine($"  Generic argument: {arg.Name}");
        }
    }

    public void CustomTypeTests()
    {
        // Test with custom types
        var person = new Person { Name = "John", Age = 30 };
        var personTypeInfo = person.GetTypeGen();
        Console.WriteLine($"Person type name: {personTypeInfo.Name}");
        Console.WriteLine($"Person is reference type: {personTypeInfo.IsReferenceType}");

        // Test properties
        var properties = personTypeInfo.Properties;
        Console.WriteLine($"Person properties count: {properties.Length}");
        foreach (var prop in properties)
        {
            Console.WriteLine($"  Property: {prop.Name} ({prop.PropertyType.Name})");
        }

        // Test fields
        var fields = personTypeInfo.Fields;
        Console.WriteLine($"Person fields count: {fields.Length}");
        foreach (var field in fields)
        {
            Console.WriteLine($"  Field: {field.Name} ({field.FieldType.Name})");
        }
    }

    public void MethodTests()
    {
        var person = new Person { Name = "Jane", Age = 25 };
        var personTypeInfo = person.GetTypeGen();

        // Test methods
        var methods = personTypeInfo.Methods;
        Console.WriteLine($"Person methods count: {methods.Length}");
        foreach (var method in methods)
        {
            Console.WriteLine($"  Method: {method.Name} (Return: {method.ReturnType.Name})");
        }

        // Test specific method
        var toStringMethod = personTypeInfo.GetMethod("ToString");
        if (toStringMethod.HasValue)
        {
            Console.WriteLine($"ToString method found: {toStringMethod.Value.Name}");
            Console.WriteLine($"ToString return type: {toStringMethod.Value.ReturnType.Name}");
        }
    }

    public void PropertyAccessTests()
    {
        var person = new Person { Name = "Bob", Age = 35 };
        var personTypeInfo = person.GetTypeGen();

        // Test property access
        var nameProperty = personTypeInfo.GetProperty("Name");
        if (nameProperty.HasValue)
        {
            Console.WriteLine($"Name property found: {nameProperty.Value.Name}");
            Console.WriteLine($"Name property type: {nameProperty.Value.PropertyType.Name}");
            Console.WriteLine($"Name property can read: {nameProperty.Value.CanRead}");
            Console.WriteLine($"Name property can write: {nameProperty.Value.CanWrite}");

            // Test getting property value
            var nameValue = nameProperty.Value.GetValue(person);
            Console.WriteLine($"Name property value: {nameValue}");
        }
    }

    public void FieldAccessTests()
    {
        var person = new Person { Name = "Alice", Age = 28 };
        var personTypeInfo = person.GetTypeGen();

        // Test field access
        var ageField = personTypeInfo.GetField("Age");
        if (ageField.HasValue)
        {
            Console.WriteLine($"Age field found: {ageField.Value.Name}");
            Console.WriteLine($"Age field type: {ageField.Value.FieldType.Name}");
            Console.WriteLine($"Age field is readonly: {ageField.Value.IsReadOnly}");

            // Test getting field value
            var ageValue = ageField.Value.GetValue(person);
            Console.WriteLine($"Age field value: {ageValue}");
        }
    }

    public void ComplexTypeTests()
    {
        // Test with complex nested types
        var company = new Company
        {
            Name = "Test Corp",
            Employees = new List<Person>
            {
                new Person { Name = "Employee1", Age = 30 },
                new Person { Name = "Employee2", Age = 25 }
            }
        };

        var companyTypeInfo = company.GetTypeGen();
        Console.WriteLine($"Company type name: {companyTypeInfo.Name}");

        // Test nested property access
        var employeesProperty = companyTypeInfo.GetProperty("Employees");
        if (employeesProperty.HasValue)
        {
            Console.WriteLine($"Employees property type: {employeesProperty.Value.PropertyType.Name}");
            Console.WriteLine($"Employees property is generic: {employeesProperty.Value.PropertyType.IsGenericType}");
        }
    }
}

/// <summary>
/// Sample Person class for testing
/// </summary>
public class Person
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
/// Sample Company class for testing
/// </summary>
public class Company
{
    public string Name { get; set; } = string.Empty;
    public List<Person> Employees { get; set; } = new();
    public DateTime FoundedDate { get; set; }
}

