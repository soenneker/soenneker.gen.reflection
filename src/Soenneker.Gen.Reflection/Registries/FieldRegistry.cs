using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Soenneker.Gen.Reflection.Dtos;

namespace Soenneker.Gen.Reflection.Registries;

/// <summary>
/// Global registry for precomputed FieldInfoGen instances
/// </summary>
public static class FieldRegistry
{
    private static readonly Dictionary<ulong, FieldInfoGen> _fields = new();
    private static readonly Dictionary<ulong, Dictionary<string, ulong>> _typeToFieldIds = new();

    /// <summary>
    /// Registers a field with its ID
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Register(ulong fieldId, FieldInfoGen fieldInfo, ulong typeId)
    {
        _fields[fieldId] = fieldInfo;
        
        if (!_typeToFieldIds.TryGetValue(typeId, out var fieldMap))
        {
            fieldMap = new Dictionary<string, ulong>();
            _typeToFieldIds[typeId] = fieldMap;
        }
        fieldMap[fieldInfo.Name] = fieldId;
    }

    /// <summary>
    /// Gets a field by its ID
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FieldInfoGen GetField(ulong fieldId)
    {
        return _fields.TryGetValue(fieldId, out var field) ? field : default;
    }

    /// <summary>
    /// Gets a field by type ID and name using perfect hash switch
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FieldInfoGen? GetFieldByName(ulong typeId, string name)
    {
        if (_typeToFieldIds.TryGetValue(typeId, out var fieldMap) && 
            fieldMap.TryGetValue(name, out var fieldId))
        {
            return GetField(fieldId);
        }
        return null;
    }

    /// <summary>
    /// Gets all fields for a type
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<FieldInfoGen> GetFieldsForType(ulong typeId)
    {
        if (!_typeToFieldIds.TryGetValue(typeId, out var fieldMap))
            return ReadOnlySpan<FieldInfoGen>.Empty;

        var result = new FieldInfoGen[fieldMap.Count];
        int index = 0;
        foreach (var fieldId in fieldMap.Values)
        {
            result[index++] = GetField(fieldId);
        }
        return result;
    }
}
