[![](https://img.shields.io/nuget/v/soenneker.gen.reflection.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.gen.reflection/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.gen.reflection/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.gen.reflection/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.gen.reflection.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.gen.reflection/)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Gen.Reflection
### Compile-time reflection for .NET

## Installation

```
dotnet add package Soenneker.Gen.Reflection
```

## Features

### GetTypeGen Extension Method

The `GetTypeGen()` extension method provides compile-time generated type information without runtime reflection overhead. This source generator analyzes your code and generates highly optimized non-reflection based code for type introspection.

#### Basic Usage

```csharp
using Soenneker.Gen.Reflection;

// Get type information from an instance
string text = "Hello World";
var typeInfo = text.GetTypeGen();

Console.WriteLine($"Type: {typeInfo.Name}");           // "String"
Console.WriteLine($"Is Value Type: {typeInfo.IsValueType}"); // false
Console.WriteLine($"Is Reference Type: {typeInfo.IsReferenceType}"); // true
```

#### Generic Method Usage

```csharp
// Get type information using generic method
var listTypeInfo = GetTypeGen<List<string>>();
Console.WriteLine($"Type: {listTypeInfo.Name}"); // "List`1"
Console.WriteLine($"Is Generic: {listTypeInfo.IsGenericType}"); // true
Console.WriteLine($"Generic Args: {listTypeInfo.GenericTypeArguments.Length}"); // 1
```

#### Property and Field Access

```csharp
public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
    private readonly string _id = Guid.NewGuid().ToString();
}

var person = new Person { Name = "John", Age = 30 };
var personTypeInfo = person.GetTypeGen();

// Access properties
var nameProperty = personTypeInfo.GetProperty("Name");
if (nameProperty.HasValue)
{
    Console.WriteLine($"Property: {nameProperty.Value.Name}");
    Console.WriteLine($"Type: {nameProperty.Value.PropertyType.Name}");
    Console.WriteLine($"Value: {nameProperty.Value.GetValue(person)}");
}

// Access fields
var fields = personTypeInfo.Fields;
foreach (var field in fields)
{
    Console.WriteLine($"Field: {field.Name} ({field.FieldType.Name})");
}
```

#### Method Information

```csharp
var personTypeInfo = person.GetTypeGen();
var toStringMethod = personTypeInfo.GetMethod("ToString");

if (toStringMethod.HasValue)
{
    Console.WriteLine($"Method: {toStringMethod.Value.Name}");
    Console.WriteLine($"Return Type: {toStringMethod.Value.ReturnType.Name}");
    Console.WriteLine($"Parameters: {toStringMethod.Value.ParameterTypes.Length}");
    
    // Invoke the method
    var result = toStringMethod.Value.Invoke(person, null);
    Console.WriteLine($"Result: {result}");
}
```

#### Nullable Types

```csharp
int? nullableInt = 42;
var nullableTypeInfo = nullableInt.GetTypeGen();

Console.WriteLine($"Type: {nullableTypeInfo.Name}"); // "Nullable`1"
Console.WriteLine($"Is Nullable: {nullableTypeInfo.IsNullable}"); // true

if (nullableTypeInfo.UnderlyingType.HasValue)
{
    Console.WriteLine($"Underlying: {nullableTypeInfo.UnderlyingType.Value.Name}"); // "Int32"
}
```

#### Razor/Blazor Support

The source generator also works in Razor components:

```razor
@{
    string text = "Hello Blazor";
    var typeInfo = text.GetTypeGen();
}

<p>Type: @typeInfo.Name</p>
<p>Is Reference Type: @typeInfo.IsReferenceType</p>
```

## Generated Code

The source generator creates optimized code at compile time. For example, when you call `text.GetTypeGen()`, the generator creates:

```csharp
// Generated code (simplified)
public static partial class StringTypeInfo
{
    public static string Name => "String";
    public static string FullName => "System.String";
    public static bool IsValueType => false;
    public static bool IsReferenceType => true;
    public static bool IsGenericType => false;
    public static bool IsNullable => false;
    // ... more properties
}
```

## Performance Benefits

- **Zero Runtime Reflection**: All type information is generated at compile time
- **Optimized Code**: Direct property access instead of reflection calls
- **Type Safety**: Compile-time type checking and IntelliSense support
- **Minimal Memory Overhead**: No reflection metadata loaded at runtime

## Supported Types

- Primitive types (int, string, bool, etc.)
- Nullable types (int?, DateTime?, etc.)
- Generic types (List<T>, Dictionary<K,V>, etc.)
- Custom classes and structs
- Arrays
- Enums
- Complex nested types

## Requirements

- .NET 6.0 or later
- C# 9.0 or later
- Visual Studio 2022 or VS Code with C# extension