using System;
using System.Runtime.CompilerServices;
using Soenneker.Gen.Reflection.Registries;

namespace Soenneker.Gen.Reflection.Dtos;

/// <summary>
/// Represents compile-time generated field information
/// </summary>
public readonly struct FieldInfoGen
{
    private readonly ulong _id;
    private readonly string _name;
    private readonly string _fieldTypeName;
    private readonly ulong _fieldTypeId;
    private readonly bool _isReadOnly;
    private readonly Func<object?, object?>? _getter;
    private readonly Action<object?, object?>? _setter;

    public FieldInfoGen(string name, string fieldTypeName, bool isReadOnly)
    {
        _id = 0UL;
        _name = name;
        _fieldTypeName = fieldTypeName;
        _fieldTypeId = 0UL;
        _isReadOnly = isReadOnly;
        _getter = null;
        _setter = null;
    }

    public FieldInfoGen(string name, string fieldTypeName, bool isReadOnly, Func<object?, object?>? getter, Action<object?, object?>? setter)
    {
        _id = 0UL;
        _name = name;
        _fieldTypeName = fieldTypeName;
        _fieldTypeId = 0UL;
        _isReadOnly = isReadOnly;
        _getter = getter;
        _setter = setter;
    }

    public FieldInfoGen(ulong id, string name, ulong fieldTypeId, bool isReadOnly)
    {
        _id = id;
        _name = name;
        _fieldTypeName = string.Empty;
        _fieldTypeId = fieldTypeId;
        _isReadOnly = isReadOnly;
        _getter = null;
        _setter = null;
    }

    public FieldInfoGen(ulong id, string name, ulong fieldTypeId, bool isReadOnly, Func<object?, object?>? getter, Action<object?, object?>? setter)
    {
        _id = id;
        _name = name;
        _fieldTypeName = string.Empty;
        _fieldTypeId = fieldTypeId;
        _isReadOnly = isReadOnly;
        _getter = getter;
        _setter = setter;
    }

    public ulong Id => _id;
    public string Name => _name;
    public TypeInfoGen FieldType => _fieldTypeId != 0UL
        ? TypeRegistry.GetType(_fieldTypeId)
        : new TypeInfoGen(0UL, _fieldTypeName, _fieldTypeName, _fieldTypeName, false, true, false, false, Array.Empty<ulong>(), Array.Empty<ulong>(), Array.Empty<ulong>(), null, null);
    public bool IsReadOnly => _isReadOnly;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public object? GetValue(object? obj)
    {
        if (_getter != null)
            return _getter(obj);
        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetValue(object? obj, object? value)
    {
        _setter?.Invoke(obj, value);
    }
}
