using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Soenneker.Gen.Reflection.Dtos;

namespace Soenneker.Gen.Reflection.Registries;

/// <summary>
/// Global registry for precomputed PropertyInfoGen instances
/// </summary>
public static class PropertyRegistry
{
    private static readonly Dictionary<ulong, PropertyInfoGen> _properties = new();
    private static readonly Dictionary<ulong, Dictionary<string, ulong>> _typeToPropertyIds = new();

    /// <summary>
    /// Registers a property with its ID
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Register(ulong propertyId, PropertyInfoGen propertyInfo, ulong typeId)
    {
        _properties[propertyId] = propertyInfo;
        
        if (!_typeToPropertyIds.TryGetValue(typeId, out var propertyMap))
        {
            propertyMap = new Dictionary<string, ulong>();
            _typeToPropertyIds[typeId] = propertyMap;
        }
        propertyMap[propertyInfo.Name] = propertyId;
    }

    /// <summary>
    /// Gets a property by its ID
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PropertyInfoGen GetProperty(ulong propertyId)
    {
        return _properties.TryGetValue(propertyId, out var property) ? property : default;
    }

    /// <summary>
    /// Gets a property by type ID and name using perfect hash switch
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PropertyInfoGen? GetPropertyByName(ulong typeId, string name)
    {
        if (_typeToPropertyIds.TryGetValue(typeId, out var propertyMap) && 
            propertyMap.TryGetValue(name, out var propertyId))
        {
            return GetProperty(propertyId);
        }
        return null;
    }

    /// <summary>
    /// Gets all properties for a type
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<PropertyInfoGen> GetPropertiesForType(ulong typeId)
    {
        if (!_typeToPropertyIds.TryGetValue(typeId, out var propertyMap))
            return ReadOnlySpan<PropertyInfoGen>.Empty;

        var result = new PropertyInfoGen[propertyMap.Count];
        int index = 0;
        foreach (var propertyId in propertyMap.Values)
        {
            result[index++] = GetProperty(propertyId);
        }
        return result;
    }
}
