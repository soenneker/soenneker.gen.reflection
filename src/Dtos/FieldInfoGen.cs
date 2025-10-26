using System;

namespace Soenneker.Gen.Reflection.Dtos;

/// <summary>
/// Represents compile-time generated field information
/// </summary>
public readonly struct FieldInfoGen
{
    private readonly string _name;
    private readonly string _fieldTypeName;
    private readonly bool _isReadOnly;
    private readonly Func<object?, object?>? _getter;
    private readonly Action<object?, object?>? _setter;

    public FieldInfoGen(string name, string fieldTypeName, bool isReadOnly)
    {
        _name = name;
        _fieldTypeName = fieldTypeName;
        _isReadOnly = isReadOnly;
        _getter = null;
        _setter = null;
    }

    public FieldInfoGen(string name, string fieldTypeName, bool isReadOnly, Func<object?, object?>? getter, Action<object?, object?>? setter)
    {
        _name = name;
        _fieldTypeName = fieldTypeName;
        _isReadOnly = isReadOnly;
        _getter = getter;
        _setter = setter;
    }

    public string Name => _name;
    public TypeInfoGen FieldType => new TypeInfoGen(_fieldTypeName, _fieldTypeName, _fieldTypeName, false, true, false, false, Array.Empty<FieldInfoGen>(), Array.Empty<PropertyInfoGen>(), Array.Empty<MethodInfoGen>(), null, null);
    public bool IsReadOnly => _isReadOnly;

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
