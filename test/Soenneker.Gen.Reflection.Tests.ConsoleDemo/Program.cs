using System;
using System.Collections.Generic;
using Soenneker.Gen.Reflection;

namespace Soenneker.Gen.Reflection.Tests.ConsoleDemo;

/// <summary>
/// Console application demonstrating GetTypeGen functionality
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== GetTypeGen Source Generator Demo ===\n");

        // Test primitive types
        TestPrimitiveTypes();
        
        // Test nullable types
        TestNullableTypes();
        
        // Test generic types
        TestGenericTypes();
        
        // Test custom types
        TestCustomTypes();
        
        // Test property and field access
        TestPropertyFieldAccess();
        
        // Test method information
        TestMethodInformation();
        
        // Test complex nested types
        TestComplexTypes();

        Console.WriteLine("\n=== Demo Complete ===");
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    static void TestPrimitiveTypes()
    {
        Console.WriteLine("--- Primitive Types ---");
        
        int intValue = 42;
        string stringValue = "Hello World";
        bool boolValue = true;
        double doubleValue = 3.14159;

        var intTypeInfo = intValue.GetTypeGen();
        var stringTypeInfo = stringValue.GetTypeGen();
        var boolTypeInfo = boolValue.GetTypeGen();
        var doubleTypeInfo = doubleValue.GetTypeGen();

        Console.WriteLine($"int: {intTypeInfo.Name} (Value: {intTypeInfo.IsValueType}, Ref: {intTypeInfo.IsReferenceType})");
        Console.WriteLine($"string: {stringTypeInfo.Name} (Value: {stringTypeInfo.IsValueType}, Ref: {stringTypeInfo.IsReferenceType})");
        Console.WriteLine($"bool: {boolTypeInfo.Name} (Value: {boolTypeInfo.IsValueType}, Ref: {boolTypeInfo.IsReferenceType})");
        Console.WriteLine($"double: {doubleTypeInfo.Name} (Value: {doubleTypeInfo.IsValueType}, Ref: {doubleTypeInfo.IsReferenceType})");
        Console.WriteLine();
    }

    static void TestNullableTypes()
    {
        Console.WriteLine("--- Nullable Types ---");
        
        int? nullableInt = 42;
        DateTime? nullableDateTime = DateTime.Now;
        string? nullableString = "test";

        var nullableIntTypeInfo = nullableInt.GetTypeGen();
        var nullableDateTimeTypeInfo = nullableDateTime.GetTypeGen();
        var nullableStringTypeInfo = nullableString.GetTypeGen();

        Console.WriteLine($"int?: {nullableIntTypeInfo.Name} (Nullable: {nullableIntTypeInfo.IsNullable})");
        if (nullableIntTypeInfo.UnderlyingType.HasValue)
        {
            Console.WriteLine($"  Underlying: {nullableIntTypeInfo.UnderlyingType.Value.Name}");
        }

        Console.WriteLine($"DateTime?: {nullableDateTimeTypeInfo.Name} (Nullable: {nullableDateTimeTypeInfo.IsNullable})");
        if (nullableDateTimeTypeInfo.UnderlyingType.HasValue)
        {
            Console.WriteLine($"  Underlying: {nullableDateTimeTypeInfo.UnderlyingType.Value.Name}");
        }

        Console.WriteLine($"string?: {nullableStringTypeInfo.Name} (Nullable: {nullableStringTypeInfo.IsNullable})");
        Console.WriteLine();
    }

    static void TestGenericTypes()
    {
        Console.WriteLine("--- Generic Types ---");
        
        var list = new List<string>();
        var dict = new Dictionary<string, int>();
        var hashSet = new HashSet<DateTime>();

        var listTypeInfo = list.GetTypeGen();
        var dictTypeInfo = dict.GetTypeGen();
        var hashSetTypeInfo = hashSet.GetTypeGen();

        Console.WriteLine($"List<string>: {listTypeInfo.Name} (Generic: {listTypeInfo.IsGenericType})");
        Console.WriteLine($"  Generic args: {listTypeInfo.GenericTypeArguments.Length}");
        foreach (var arg in listTypeInfo.GenericTypeArguments)
        {
            Console.WriteLine($"    - {arg.Name}");
        }

        Console.WriteLine($"Dictionary<string,int>: {dictTypeInfo.Name} (Generic: {dictTypeInfo.IsGenericType})");
        Console.WriteLine($"  Generic args: {dictTypeInfo.GenericTypeArguments.Length}");
        foreach (var arg in dictTypeInfo.GenericTypeArguments)
        {
            Console.WriteLine($"    - {arg.Name}");
        }

        Console.WriteLine($"HashSet<DateTime>: {hashSetTypeInfo.Name} (Generic: {hashSetTypeInfo.IsGenericType})");
        Console.WriteLine();
    }

    static void TestCustomTypes()
    {
        Console.WriteLine("--- Custom Types ---");
        
        var person = new Person { Name = "John Doe", Age = 30, Email = "john@example.com" };
        var personTypeInfo = person.GetTypeGen();

        Console.WriteLine($"Person: {personTypeInfo.Name}");
        Console.WriteLine($"  Full Name: {personTypeInfo.FullName}");
        Console.WriteLine($"  Is Value Type: {personTypeInfo.IsValueType}");
        Console.WriteLine($"  Is Reference Type: {personTypeInfo.IsReferenceType}");
        Console.WriteLine($"  Is Generic: {personTypeInfo.IsGenericType}");
        Console.WriteLine($"  Properties: {personTypeInfo.Properties.Length}");
        Console.WriteLine($"  Fields: {personTypeInfo.Fields.Length}");
        Console.WriteLine($"  Methods: {personTypeInfo.Methods.Length}");
        Console.WriteLine();
    }

    static void TestPropertyFieldAccess()
    {
        Console.WriteLine("--- Property and Field Access ---");
        
        var person = new Person { Name = "Jane Smith", Age = 25, Email = "jane@example.com" };
        var personTypeInfo = person.GetTypeGen();

        // Test property access
        var nameProperty = personTypeInfo.GetProperty("Name");
        if (nameProperty.HasValue)
        {
            Console.WriteLine($"Name Property:");
            Console.WriteLine($"  Name: {nameProperty.Value.Name}");
            Console.WriteLine($"  Type: {nameProperty.Value.PropertyType.Name}");
            Console.WriteLine($"  Can Read: {nameProperty.Value.CanRead}");
            Console.WriteLine($"  Can Write: {nameProperty.Value.CanWrite}");
            Console.WriteLine($"  Value: {nameProperty.Value.GetValue(person)}");
        }

        var ageProperty = personTypeInfo.GetProperty("Age");
        if (ageProperty.HasValue)
        {
            Console.WriteLine($"Age Property:");
            Console.WriteLine($"  Name: {ageProperty.Value.Name}");
            Console.WriteLine($"  Type: {ageProperty.Value.PropertyType.Name}");
            Console.WriteLine($"  Value: {ageProperty.Value.GetValue(person)}");
        }

        // Test field access
        var fields = personTypeInfo.Fields;
        Console.WriteLine($"Fields ({fields.Length}):");
        foreach (var field in fields)
        {
            Console.WriteLine($"  {field.Name} ({field.FieldType.Name}) - ReadOnly: {field.IsReadOnly}");
        }
        Console.WriteLine();
    }

    static void TestMethodInformation()
    {
        Console.WriteLine("--- Method Information ---");
        
        var person = new Person { Name = "Bob Johnson", Age = 40, Email = "bob@example.com" };
        var personTypeInfo = person.GetTypeGen();

        var toStringMethod = personTypeInfo.GetMethod("ToString");
        if (toStringMethod.HasValue)
        {
            Console.WriteLine($"ToString Method:");
            Console.WriteLine($"  Name: {toStringMethod.Value.Name}");
            Console.WriteLine($"  Return Type: {toStringMethod.Value.ReturnType.Name}");
            Console.WriteLine($"  Is Static: {toStringMethod.Value.IsStatic}");
            Console.WriteLine($"  Parameters: {toStringMethod.Value.ParameterTypes.Length}");
            
            // Test method invocation
            var result = toStringMethod.Value.Invoke(person, null);
            Console.WriteLine($"  Invoke Result: {result}");
        }

        var equalsMethod = personTypeInfo.GetMethod("Equals");
        if (equalsMethod.HasValue)
        {
            Console.WriteLine($"Equals Method:");
            Console.WriteLine($"  Name: {equalsMethod.Value.Name}");
            Console.WriteLine($"  Return Type: {equalsMethod.Value.ReturnType.Name}");
            Console.WriteLine($"  Parameters: {equalsMethod.Value.ParameterTypes.Length}");
        }
        Console.WriteLine();
    }

    static void TestComplexTypes()
    {
        Console.WriteLine("--- Complex Nested Types ---");
        
        var company = new Company
        {
            Name = "Tech Solutions Inc",
            FoundedDate = DateTime.Now.AddYears(-10),
            Employees = new List<Person>
            {
                new Person { Name = "Alice Developer", Age = 28, Email = "alice@tech.com" },
                new Person { Name = "Bob Manager", Age = 35, Email = "bob@tech.com" },
                new Person { Name = "Charlie Designer", Age = 32, Email = "charlie@tech.com" }
            },
            Departments = new Dictionary<string, List<Person>>
            {
                ["Engineering"] = new List<Person>
                {
                    new Person { Name = "Alice Developer", Age = 28, Email = "alice@tech.com" }
                },
                ["Management"] = new List<Person>
                {
                    new Person { Name = "Bob Manager", Age = 35, Email = "bob@tech.com" }
                }
            }
        };

        var companyTypeInfo = company.GetTypeGen();
        Console.WriteLine($"Company: {companyTypeInfo.Name}");
        Console.WriteLine($"  Properties: {companyTypeInfo.Properties.Length}");
        
        foreach (var prop in companyTypeInfo.Properties)
        {
            Console.WriteLine($"    {prop.Name} ({prop.PropertyType.Name})");
            
            // Check if it's a generic property
            if (prop.PropertyType.IsGenericType)
            {
                Console.WriteLine($"      Generic Args: {prop.PropertyType.GenericTypeArguments.Length}");
                foreach (var arg in prop.PropertyType.GenericTypeArguments)
                {
                    Console.WriteLine($"        - {arg.Name}");
                }
            }
        }

        // Test nested property access
        var employeesProperty = companyTypeInfo.GetProperty("Employees");
        if (employeesProperty.HasValue)
        {
            Console.WriteLine($"Employees Property:");
            Console.WriteLine($"  Type: {employeesProperty.Value.PropertyType.Name}");
            Console.WriteLine($"  Is Generic: {employeesProperty.Value.PropertyType.IsGenericType}");
            Console.WriteLine($"  Generic Args: {employeesProperty.Value.PropertyType.GenericTypeArguments.Length}");
            
            var employeesValue = employeesProperty.Value.GetValue(company);
            if (employeesValue is List<Person> employees)
            {
                Console.WriteLine($"  Employee Count: {employees.Count}");
            }
        }
        Console.WriteLine();
    }
}

/// <summary>
/// Sample Person class for demonstration
/// </summary>
public class Person
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Email { get; set; } = string.Empty;
    private readonly string _id = Guid.NewGuid().ToString();
    private DateTime _createdAt = DateTime.Now;

    public override string ToString()
    {
        return $"{Name} ({Age}) - {Email}";
    }

    public bool IsAdult()
    {
        return Age >= 18;
    }
}

/// <summary>
/// Sample Company class for demonstration
/// </summary>
public class Company
{
    public string Name { get; set; } = string.Empty;
    public DateTime FoundedDate { get; set; }
    public List<Person> Employees { get; set; } = new();
    public Dictionary<string, List<Person>> Departments { get; set; } = new();
    private readonly string _companyId = Guid.NewGuid().ToString();

    public int GetEmployeeCount()
    {
        return Employees.Count;
    }

    public List<Person> GetEmployeesByDepartment(string department)
    {
        return Departments.TryGetValue(department, out var employees) ? employees : new List<Person>();
    }
}

