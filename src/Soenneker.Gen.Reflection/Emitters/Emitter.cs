using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Linq;
using System.Text;

namespace Soenneker.Gen.Reflection.Emitters;

/// <summary>
/// Base emitter class providing common functionality for all emitters
/// </summary>
internal static class Emitter
{
    /// <summary>
    /// Adds generated source to the compilation
    /// </summary>
    public static void AddSource(SourceProductionContext context, string fileName, StringBuilder stringBuilder)
    {
        string content = stringBuilder.ToString().Replace("\r\n", "\n");
        context.AddSource(fileName, SourceText.From(content, Encoding.UTF8));
    }

    /// <summary>
    /// Gets the target namespace from the compilation
    /// </summary>
    public static string GetTargetNamespace(Compilation compilation)
    {
        // Use the assembly name as the namespace
        // Dots are valid in namespaces, so we keep them
        string assemblyName = compilation.AssemblyName ?? "GeneratedReflection";

        // Only clean up characters that are invalid in namespaces
        assemblyName = assemblyName.Replace(" ", "").Replace("-", "_");

        return assemblyName;
    }

    /// <summary>
    /// Sanitizes a type name for use in generated class names
    /// </summary>
    public static string SanitizeTypeName(string typeName)
    {
        // Create a more sophisticated sanitization that preserves uniqueness
        string sanitized = typeName
            .Replace("<", "Of")
            .Replace(">", "")
            .Replace(",", "And")
            .Replace(" ", "")
            .Replace(".", "")
            .Replace("[", "Array")
            .Replace("]", "")
            .Replace("?", "Nullable")
            .Replace("&", "Ref");
        
        // Ensure it starts with a letter
        if (char.IsDigit(sanitized[0]))
        {
            sanitized = "T" + sanitized;
        }
        
        return sanitized;
    }

    /// <summary>
    /// Gets the type name from a type symbol
    /// </summary>
    public static string GetTypeName(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is IArrayTypeSymbol arrayType)
        {
            return GetTypeName(arrayType.ElementType) + "[]";
        }

        if (typeSymbol is INamedTypeSymbol namedType && namedType.IsGenericType)
        {
            return namedType.Name + "`" + namedType.TypeParameters.Length;
        }
        
        // Handle special cases for built-in types
        return typeSymbol.SpecialType switch
        {
            SpecialType.System_String => "String",
            SpecialType.System_Int32 => "Int32",
            SpecialType.System_Boolean => "Boolean",
            SpecialType.System_Object => "Object",
            SpecialType.System_Void => "Void",
            _ => typeSymbol.Name
        };
    }

    /// <summary>
    /// Formats a type symbol as fully qualified
    /// </summary>
    public static string FormatFullyQualified(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is IArrayTypeSymbol arrayType)
        {
            return FormatFullyQualified(arrayType.ElementType) + "[]";
        }

        if (typeSymbol is INamedTypeSymbol named)
        {
            string ns = string.IsNullOrEmpty(named.ContainingNamespace?.ToDisplayString())
                ? ""
                : "global::" + named.ContainingNamespace.ToDisplayString() + ".";
            string name = named.Name;
            if (named.IsGenericType)
            {
                string args = string.Join(", ", named.TypeArguments.Select(FormatFullyQualified));
                return ns + name + "<" + args + ">";
            }
            return ns + name;
        }

        return typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            .Replace("global::", "global::");
    }

    /// <summary>
    /// Checks if a type symbol is nullable
    /// </summary>
    public static bool IsNullableType(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is INamedTypeSymbol namedType)
        {
            return namedType.IsGenericType && namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;
        }
        return false;
    }

    /// <summary>
    /// Gets the underlying type name for nullable types
    /// </summary>
    public static string GetUnderlyingTypeName(ITypeSymbol typeSymbol)
    {
        if (IsNullableType(typeSymbol) && typeSymbol is INamedTypeSymbol namedType)
        {
            ITypeSymbol underlyingType = namedType.TypeArguments[0];
            return $"\"{GetTypeName(underlyingType)}\"";
        }
        return "null";
    }

    /// <summary>
    /// Gets generic type argument names
    /// </summary>
    public static string GetGenericTypeArgumentNames(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is INamedTypeSymbol namedType && namedType.IsGenericType)
        {
            ITypeSymbol[] typeArguments = namedType.TypeArguments.ToArray();
            string[] names = Array.ConvertAll(typeArguments, arg => $"\"{GetTypeName(arg)}\"");
            return $"new string[] {{ {string.Join(", ", names)} }}";
        }
        return "null";
    }

    /// <summary>
    /// Gets the underlying type ID for nullable types (optimized version)
    /// </summary>
    public static string GetUnderlyingTypeId(ITypeSymbol typeSymbol)
    {
        if (IsNullableType(typeSymbol) && typeSymbol is INamedTypeSymbol namedType)
        {
            ITypeSymbol underlyingType = namedType.TypeArguments[0];
            return $"{(ulong)underlyingType.GetHashCode()}UL";
        }
        return "null";
    }

    /// <summary>
    /// Gets generic type argument IDs (optimized version)
    /// </summary>
    public static string GetGenericTypeArgumentIds(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is INamedTypeSymbol namedType && namedType.IsGenericType)
        {
            ITypeSymbol[] typeArguments = namedType.TypeArguments.ToArray();
            string[] ids = Array.ConvertAll(typeArguments, arg => $"{(ulong)arg.GetHashCode()}UL");
            return $"new ulong[] {{ {string.Join(", ", ids)} }}";
        }
        return "null";
    }
}
