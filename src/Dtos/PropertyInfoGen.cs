using System;

namespace Soenneker.Gen.Reflection.Dtos;

/// <summary>
/// Represents compile-time generated property information
/// </summary>
public readonly struct PropertyInfoGen
{
    private readonly string _name;
    private readonly TypeInfoGen _propertyType;
    private readonly bool _canRead;
    private readonly bool _canWrite;
    private readonly Func<object?, object?>? _getter;
    private readonly Action<object?, object?>? _setter;

    public PropertyInfoGen(string name, string propertyTypeName, bool canRead, bool canWrite)
    {
        _name = name;
        _propertyType = new TypeInfoGen(propertyTypeName, propertyTypeName, propertyTypeName, false, true, propertyTypeName.Contains("`"), false, Array.Empty<FieldInfoGen>(), Array.Empty<PropertyInfoGen>(), Array.Empty<MethodInfoGen>(), null, null);
        _canRead = canRead;
        _canWrite = canWrite;
        _getter = null;
        _setter = null;
    }

    public PropertyInfoGen(string name, TypeInfoGen propertyType, bool canRead, bool canWrite, Func<object?, object?>? getter, Action<object?, object?>? setter)
    {
        _name = name;
        _propertyType = propertyType;
        _canRead = canRead;
        _canWrite = canWrite;
        _getter = getter;
        _setter = setter;
    }

    public PropertyInfoGen(string name, string propertyTypeName, bool canRead, bool canWrite, Func<object?, object?>? getter, Action<object?, object?>? setter)
    {
        _name = name;
        _propertyType = new TypeInfoGen(propertyTypeName, propertyTypeName, propertyTypeName, false, true, propertyTypeName.Contains("`"), false, Array.Empty<FieldInfoGen>(), Array.Empty<PropertyInfoGen>(), Array.Empty<MethodInfoGen>(), null, null);
        _canRead = canRead;
        _canWrite = canWrite;
        _getter = getter;
        _setter = setter;
    }

    public string Name => _name;
    public TypeInfoGen PropertyType => _propertyType;
    public bool CanRead => _canRead;
    public bool CanWrite => _canWrite;

    public object? GetValue(object? obj)
    {
        if (_getter != null)
            return _getter(obj);
        return null;
    }

    public void SetValue(object? obj, object? value)
    {
        _setter?.Invoke(obj, value);
    }
}
