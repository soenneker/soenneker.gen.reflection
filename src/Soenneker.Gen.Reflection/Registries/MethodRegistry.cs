using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Soenneker.Gen.Reflection.Dtos;

namespace Soenneker.Gen.Reflection.Registries;

/// <summary>
/// Global registry for precomputed MethodInfoGen instances
/// </summary>
public static class MethodRegistry
{
    private static readonly Dictionary<ulong, MethodInfoGen> _methods = new();
    private static readonly Dictionary<ulong, Dictionary<string, ulong>> _typeToMethodIds = new();

    /// <summary>
    /// Registers a method with its ID
    /// </summary>
    /// <param name="methodId">Identifier of the method to target.</param>
    /// <param name="methodInfo">Method Info for the register operation.</param>
    /// <param name="typeId">Identifier of the type to target.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Register(ulong methodId, MethodInfoGen methodInfo, ulong typeId)
    {
        _methods[methodId] = methodInfo;
        
        if (!_typeToMethodIds.TryGetValue(typeId, out var methodMap))
        {
            methodMap = new Dictionary<string, ulong>();
            _typeToMethodIds[typeId] = methodMap;
        }
        methodMap[methodInfo.Name] = methodId;
    }

    /// <summary>
    /// Gets a method by its ID
    /// </summary>
    /// <param name="methodId">Identifier of the method to target.</param>
    /// <returns>The requested method Info Gen.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MethodInfoGen GetMethod(ulong methodId)
    {
        return _methods.TryGetValue(methodId, out var method) ? method : default;
    }

    /// <summary>
    /// Gets a method by type ID and name using perfect hash switch
    /// </summary>
    /// <param name="typeId">Identifier of the type to target.</param>
    /// <param name="name">Name of the Method Registry value to target.</param>
    /// <returns>The requested method Info Gen.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MethodInfoGen? GetMethodByName(ulong typeId, string name)
    {
        if (_typeToMethodIds.TryGetValue(typeId, out var methodMap) && 
            methodMap.TryGetValue(name, out var methodId))
        {
            return GetMethod(methodId);
        }
        return null;
    }

    /// <summary>
    /// Gets all methods for a type
    /// </summary>
    /// <param name="typeId">Identifier of the type to target.</param>
    /// <returns>The requested read Only Span.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<MethodInfoGen> GetMethodsForType(ulong typeId)
    {
        if (!_typeToMethodIds.TryGetValue(typeId, out var methodMap))
            return ReadOnlySpan<MethodInfoGen>.Empty;

        var result = new MethodInfoGen[methodMap.Count];
        int index = 0;
        foreach (var methodId in methodMap.Values)
        {
            result[index++] = GetMethod(methodId);
        }
        return result;
    }
}
