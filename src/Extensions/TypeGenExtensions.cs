using Soenneker.Gen.Reflection.Dtos;
using System;

namespace Soenneker.Gen.Reflection.Extensions;

/// <summary>
/// Extension methods for compile-time type generation
/// </summary>
public static partial class TypeGenExtensions
{
    /// <summary>
    /// Gets compile-time generated type information for the specified object.
    /// This method will be replaced by the source generator with optimized non-reflection code.
    /// </summary>
    /// <typeparam name="T">The type to get information for</typeparam>
    /// <param name="obj">The object instance</param>
    /// <returns>Generated type information</returns>
    public static TypeInfoGen GetTypeGen<T>(this T obj)
    {
        return BuildFromType(typeof(T));
    }

    /// <summary>
    /// Gets compile-time generated type information for the specified type.
    /// This method will be replaced by the source generator with optimized non-reflection code.
    /// </summary>
    /// <typeparam name="T">The type to get information for</typeparam>
    /// <returns>Generated type information</returns>
    public static TypeInfoGen GetTypeGen<T>()
    {
        return BuildFromType(typeof(T));
    }

    private static TypeInfoGen BuildFromType(Type type)
    {
        string name;
        if (type.IsArray)
        {
            Type? elem = type.GetElementType();
            name = (elem?.Name ?? "Object") + "[]";
        }
        else if (type.IsGenericType)
        {
            name = type.Name;
        }
        else
        {
            name = type.Name;
        }

        bool isNullable = Nullable.GetUnderlyingType(type) != null;
        string? underlying = isNullable ? Nullable.GetUnderlyingType(type)?.Name : null;
        string[]? genericArgs = type.IsGenericType ? Array.ConvertAll(type.GetGenericArguments(), t => t.Name) : null;

        return new TypeInfoGen(
            0UL,
            name,
            type.FullName ?? type.Name,
            type.AssemblyQualifiedName ?? type.FullName ?? type.Name,
            type.IsValueType,
            !type.IsValueType,
            type.IsGenericType,
            isNullable,
            Array.Empty<ulong>(),
            Array.Empty<ulong>(),
            Array.Empty<ulong>(),
            null,
            null);
    }
}


