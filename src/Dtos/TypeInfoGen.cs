using System;
using System.Linq;

namespace Soenneker.Gen.Reflection.Dtos;

/// <summary>
/// Represents compile-time generated type information
/// </summary>
public readonly struct TypeInfoGen
{
    private readonly string _name;
    private readonly string _fullName;
    private readonly string _assemblyQualifiedName;
    private readonly bool _isValueType;
    private readonly bool _isReferenceType;
    private readonly bool _isGenericType;
    private readonly bool _isNullable;
    private readonly FieldInfoGen[] _fields;
    private readonly PropertyInfoGen[] _properties;
    private readonly MethodInfoGen[] _methods;
    private readonly string? _underlyingTypeName;
    private readonly string[]? _genericTypeArgumentNames;

    public TypeInfoGen(string name, string fullName, string assemblyQualifiedName, bool isValueType, bool isReferenceType, bool isGenericType, bool isNullable, FieldInfoGen[] fields, PropertyInfoGen[] properties, MethodInfoGen[] methods, string? underlyingTypeName, string[]? genericTypeArgumentNames)
    {
        _name = name;
        _fullName = fullName;
        _assemblyQualifiedName = assemblyQualifiedName;
        _isValueType = isValueType;
        _isReferenceType = isReferenceType;
        _isGenericType = isGenericType;
        _isNullable = isNullable;
        _fields = fields;
        _properties = properties;
        _methods = methods;
        _underlyingTypeName = underlyingTypeName;
        _genericTypeArgumentNames = genericTypeArgumentNames;
    }

    public string Name => _name;
    public string FullName => _fullName;
    public string AssemblyQualifiedName => _assemblyQualifiedName;
    public bool IsValueType => _isValueType;
    public bool IsReferenceType => _isReferenceType;
    public bool IsGenericType => _isGenericType;
    public bool IsNullable => _isNullable;

    public TypeInfoGen? UnderlyingType
    {
        get
        {
            if (_underlyingTypeName == null)
                return null;
            string? n = _underlyingTypeName;
            return new TypeInfoGen(n, n, n, true, false, false, false, Array.Empty<FieldInfoGen>(), Array.Empty<PropertyInfoGen>(), Array.Empty<MethodInfoGen>(), null, null);
        }
    }

    public TypeInfoGen[] GenericTypeArguments
    {
        get
        {
            if (_genericTypeArgumentNames == null || _genericTypeArgumentNames.Length == 0)
                return Array.Empty<TypeInfoGen>();
            return Array.ConvertAll(_genericTypeArgumentNames, n => new TypeInfoGen(n, n, n, true, false, false, false, Array.Empty<FieldInfoGen>(), Array.Empty<PropertyInfoGen>(), Array.Empty<MethodInfoGen>(), null, null));
        }
    }

    public PropertyInfoGen[] Properties => _properties ?? Array.Empty<PropertyInfoGen>();
    public FieldInfoGen[] Fields => _fields ?? Array.Empty<FieldInfoGen>();
    public MethodInfoGen[] Methods => _methods ?? Array.Empty<MethodInfoGen>();

    public PropertyInfoGen? GetProperty(string name)
    {
        return Properties.FirstOrDefault(p => p.Name == name);
    }

    public FieldInfoGen? GetField(string name)
    {
        return Fields.FirstOrDefault(f => f.Name == name);
    }

    public MethodInfoGen? GetMethod(string name)
    {
        return Methods.FirstOrDefault(m => m.Name == name);
    }
}
