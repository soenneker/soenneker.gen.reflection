using System;
using System.Runtime.CompilerServices;
using Soenneker.Gen.Reflection.Registries;

namespace Soenneker.Gen.Reflection.Dtos;

/// <summary>
/// Represents compile-time generated method information
/// </summary>
public readonly struct MethodInfoGen
{
    private readonly ulong _id;
    private readonly string _name;
    private readonly string _returnTypeName;
    private readonly ulong _returnTypeId;
    private readonly bool _isStatic;
    private readonly string[]? _parameterTypeNames;
    private readonly ulong[]? _parameterTypeIds;

    public MethodInfoGen(string name, string returnTypeName, bool isStatic)
    {
        _id = 0UL;
        _name = name;
        _returnTypeName = returnTypeName;
        _returnTypeId = 0UL;
        _isStatic = isStatic;
        _parameterTypeNames = Array.Empty<string>();
        _parameterTypeIds = null;
    }

    public MethodInfoGen(string name, string returnTypeName, bool isStatic, string[] parameterTypeNames)
    {
        _id = 0UL;
        _name = name;
        _returnTypeName = returnTypeName;
        _returnTypeId = 0UL;
        _isStatic = isStatic;
        _parameterTypeNames = parameterTypeNames;
        _parameterTypeIds = null;
    }

    public MethodInfoGen(ulong id, string name, ulong returnTypeId, bool isStatic, ulong[] parameterTypeIds)
    {
        _id = id;
        _name = name;
        _returnTypeName = string.Empty;
        _returnTypeId = returnTypeId;
        _isStatic = isStatic;
        _parameterTypeNames = null;
        _parameterTypeIds = parameterTypeIds;
    }

    public ulong Id => _id;
    public string Name => _name;
    public TypeInfoGen ReturnType => _returnTypeId != 0UL
        ? TypeRegistry.GetType(_returnTypeId)
        : new TypeInfoGen(0UL, _returnTypeName, _returnTypeName, _returnTypeName, false, true, false, false, Array.Empty<ulong>(), Array.Empty<ulong>(), Array.Empty<ulong>(), null, null);
    public bool IsStatic => _isStatic;
    public TypeInfoGen[] ParameterTypes
    {
        get
        {
            if (_parameterTypeIds != null)
            {
                var result = new TypeInfoGen[_parameterTypeIds.Length];
                for (int i = 0; i < result.Length; i++)
                    result[i] = TypeRegistry.GetType(_parameterTypeIds[i]);
                return result;
            }
            if (_parameterTypeNames == null || _parameterTypeNames.Length == 0)
                return Array.Empty<TypeInfoGen>();
            return Array.ConvertAll(_parameterTypeNames, n => new TypeInfoGen(0UL, n, n, n, false, true, false, false, Array.Empty<ulong>(), Array.Empty<ulong>(), Array.Empty<ulong>(), null, null));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public object? Invoke(object? obj, params object?[] parameters)
    {
        // No runtime invocation without reflection.
        return null;
    }
}
