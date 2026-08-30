[![](https://img.shields.io/nuget/v/soenneker.gen.reflection.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.gen.reflection/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.gen.reflection/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.gen.reflection/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.gen.reflection.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.gen.reflection/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.gen.reflection/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.gen.reflection/actions/workflows/codeql.yml)

# Soenneker.Gen.Reflection

A C# source generator that creates lightweight type and member metadata for types used with `GetTypeGen()`.

## Install

Add the package directly to each project containing `GetTypeGen` calls:

```bash
dotnet add package Soenneker.Gen.Reflection
```

Source generators do not flow transitively through ordinary project references.

## Usage

```csharp
using Soenneker.Gen.Reflection;

var person = new Person { Name = "Ada", Age = 37 };
TypeInfoGen type = person.GetTypeGen();

Console.WriteLine(type.Name);             // Person
Console.WriteLine(type.IsReferenceType);  // True

PropertyInfoGen? name = type.GetProperty("Name");
if (name.HasValue && name.Value.CanRead)
{
    Console.WriteLine(name.Value.GetValue(person));
}
```

The generator discovers the compile-time receiver type and emits a more-specific extension overload that returns precomputed metadata. The receiver is not inspected at runtime, so a null reference can still be used to obtain its compile-time type metadata; member getters still require a valid compatible instance.

## Available metadata

`TypeInfoGen` exposes:

- `Name`, `FullName`, and `AssemblyQualifiedName`
- value/reference, generic, and nullable flags
- nullable underlying-type and generic-argument summaries
- declared `Fields`, `Properties`, and ordinary `Methods`
- exact-name `GetField`, `GetProperty`, and `GetMethod` lookups

Public fields and properties receive generated getter delegates. Writable public fields and ordinary public setters also receive setter delegates. Non-public members remain metadata-only, and `init`-only properties are readable but are not exposed as writable.

```csharp
PropertyInfoGen age = type.GetProperty("Age").Value;
age.SetValue(person, 38);

FieldInfoGen? field = type.GetField("PublicField");
object? value = field?.GetValue(person);
```

Method metadata includes the name, return type, static flag, and parameter types. It does not invoke methods. When methods are overloaded, `GetMethod(name)` returns the first declared match; inspect `Methods` when the signature matters.

## Scope and limitations

- Metadata is generated only for types the generator can resolve from C# `GetTypeGen()` calls, plus a small set of built-in primitive types used by the generated support code.
- Member lookup is case-sensitive and covers members declared directly on the type. It is not a replacement for every `System.Reflection` binding or inheritance option.
- Property and field accessors box values through `object`; use direct access in performance-critical paths.
- Generated accessors enforce normal C# accessibility and type casts. Passing an object or assigned value of the wrong type throws the usual cast exception.
- The API does not create objects, inspect attributes, invoke methods, or access non-public values.

## Inspect generated files

```xml
<PropertyGroup>
  <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
  <CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)Generated</CompilerGeneratedFilesOutputPath>
</PropertyGroup>
```
