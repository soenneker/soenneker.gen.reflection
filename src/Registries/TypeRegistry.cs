using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Soenneker.Gen.Reflection.Dtos;

namespace Soenneker.Gen.Reflection.Registries;

/// <summary>
/// Global registry for precomputed TypeInfoGen instances
/// </summary>
public static class TypeRegistry
{
    private static readonly Dictionary<ulong, TypeInfoGen> _types = new();
    private static readonly Dictionary<string, ulong> _nameToId = new();

    /// <summary>
    /// Registers a type with its ID
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Register(ulong id, TypeInfoGen typeInfo, string name)
    {
        _types[id] = typeInfo;
        _nameToId[name] = id;
    }

    /// <summary>
    /// Gets a type by its ID
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TypeInfoGen GetType(ulong id)
    {
        return _types.TryGetValue(id, out var type) ? type : default;
    }

    /// <summary>
    /// Gets a type by its name
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TypeInfoGen? GetTypeByName(string name)
    {
        if (_nameToId.TryGetValue(name, out var id))
        {
            return GetType(id);
        }
        return null;
    }

    /// <summary>
    /// Gets all registered types
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<TypeInfoGen> GetAllTypes()
    {
        var values = new TypeInfoGen[_types.Count];
        int index = 0;
        foreach (var kvp in _types)
        {
            values[index++] = kvp.Value;
        }
        return values;
    }
}
