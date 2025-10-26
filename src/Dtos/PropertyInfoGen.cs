using System;
using System.Runtime.CompilerServices;
using Soenneker.Gen.Reflection.Registries;

namespace Soenneker.Gen.Reflection.Dtos;

/// <summary>
/// Represents compile-time generated property information
/// </summary>
public readonly struct PropertyInfoGen
{
    private readonly ulong _id;
    private readonly string _name;
    private readonly TypeInfoGen _propertyType;
    private readonly ulong _propertyTypeId;
    private readonly bool _canRead;
    private readonly bool _canWrite;
    private readonly Func<object?, object?>? _getter;
    private readonly Action<object?, object?>? _setter;

    public PropertyInfoGen(string name, string propertyTypeName, bool canRead, bool canWrite)
    {
        _id = 0UL;
        _name = name;
        _propertyType = new TypeInfoGen(0UL, propertyTypeName, propertyTypeName, propertyTypeName, false, true, propertyTypeName.Contains("`"), false, Array.Empty<ulong>(), Array.Empty<ulong>(), Array.Empty<ulong>(), null, null);
        _propertyTypeId = 0UL;
        _canRead = canRead;
        _canWrite = canWrite;
        _getter = null;
        _setter = null;
    }

    public PropertyInfoGen(string name, TypeInfoGen propertyType, bool canRead, bool canWrite, Func<object?, object?>? getter, Action<object?, object?>? setter)
    {
        _id = 0UL;
        _name = name;
        _propertyType = propertyType;
        _propertyTypeId = 0UL;
        _canRead = canRead;
        _canWrite = canWrite;
        _getter = getter;
        _setter = setter;
    }

    public PropertyInfoGen(string name, string propertyTypeName, bool canRead, bool canWrite, Func<object?, object?>? getter, Action<object?, object?>? setter)
    {
        _id = 0UL;
        _name = name;
        _propertyType = new TypeInfoGen(0UL, propertyTypeName, propertyTypeName, propertyTypeName, false, true, propertyTypeName.Contains("`"), false, Array.Empty<ulong>(), Array.Empty<ulong>(), Array.Empty<ulong>(), null, null);
        _propertyTypeId = 0UL;
        _canRead = canRead;
        _canWrite = canWrite;
        _getter = getter;
        _setter = setter;
    }

    public PropertyInfoGen(ulong id, string name, ulong propertyTypeId, bool canRead, bool canWrite)
    {
        _id = id;
        _name = name;
        _propertyTypeId = propertyTypeId;
        _propertyType = default;
        _canRead = canRead;
        _canWrite = canWrite;
        _getter = null;
        _setter = null;
    }

    public PropertyInfoGen(ulong id, string name, ulong propertyTypeId, bool canRead, bool canWrite, Func<object?, object?>? getter, Action<object?, object?>? setter)
    {
        _id = id;
        _name = name;
        _propertyTypeId = propertyTypeId;
        _propertyType = default;
        _canRead = canRead;
        _canWrite = canWrite;
        _getter = getter;
        _setter = setter;
    }

    public ulong Id => _id;
    public string Name => _name;
    public TypeInfoGen PropertyType => _propertyTypeId != 0UL
        ? TypeRegistry.GetType(_propertyTypeId)
        : _propertyType;
    public bool CanRead => _canRead;
    public bool CanWrite => _canWrite;

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
