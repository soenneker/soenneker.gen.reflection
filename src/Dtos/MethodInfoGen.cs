using System;

namespace Soenneker.Gen.Reflection.Dtos;

/// <summary>
/// Represents compile-time generated method information
/// </summary>
public readonly struct MethodInfoGen
{
    private readonly string _name;
    private readonly string _returnTypeName;
    private readonly bool _isStatic;
    private readonly string[]? _parameterTypeNames;

    public MethodInfoGen(string name, string returnTypeName, bool isStatic)
    {
        _name = name;
        _returnTypeName = returnTypeName;
        _isStatic = isStatic;
        _parameterTypeNames = Array.Empty<string>();
    }

    public MethodInfoGen(string name, string returnTypeName, bool isStatic, string[] parameterTypeNames)
    {
        _name = name;
        _returnTypeName = returnTypeName;
        _isStatic = isStatic;
        _parameterTypeNames = parameterTypeNames;
    }

    public string Name => _name;
    public TypeInfoGen ReturnType => new TypeInfoGen(_returnTypeName, _returnTypeName, _returnTypeName, false, true, false, false, Array.Empty<FieldInfoGen>(), Array.Empty<PropertyInfoGen>(), Array.Empty<MethodInfoGen>(), null, null);
    public bool IsStatic => _isStatic;
    public TypeInfoGen[] ParameterTypes => _parameterTypeNames == null || _parameterTypeNames.Length == 0
        ? Array.Empty<TypeInfoGen>()
        : Array.ConvertAll(_parameterTypeNames, n => new TypeInfoGen(n, n, n, false, true, false, false, Array.Empty<FieldInfoGen>(), Array.Empty<PropertyInfoGen>(), Array.Empty<MethodInfoGen>(), null, null));

    public object? Invoke(object? obj, params object?[] parameters)
    {
        // No runtime invocation without reflection.
        return null;
    }
}
