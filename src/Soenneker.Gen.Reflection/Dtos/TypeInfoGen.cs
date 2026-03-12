using System;
using System.Runtime.CompilerServices;
using Soenneker.Gen.Reflection.Registries;

namespace Soenneker.Gen.Reflection.Dtos;

/// <summary>
/// Represents compile-time generated type information
/// </summary>
public readonly struct TypeInfoGen
{
    private readonly ulong _id;
    private readonly string _name;
    private readonly string _fullName;
    private readonly string _assemblyQualifiedName;
    private readonly bool _isValueType;
    private readonly bool _isReferenceType;
    private readonly bool _isGenericType;
    private readonly bool _isNullable;
    private readonly ulong[] _fieldIds;
    private readonly ulong[] _propertyIds;
    private readonly ulong[] _methodIds;
    private readonly ulong? _underlyingTypeId;
    private readonly ulong[]? _genericTypeArgumentIds;

    public TypeInfoGen(ulong id, string name, string fullName, string assemblyQualifiedName, bool isValueType, bool isReferenceType, bool isGenericType, bool isNullable, ulong[] fieldIds, ulong[] propertyIds, ulong[] methodIds, ulong? underlyingTypeId, ulong[]? genericTypeArgumentIds)
    {
        _id = id;
        _name = name;
        _fullName = fullName;
        _assemblyQualifiedName = assemblyQualifiedName;
        _isValueType = isValueType;
        _isReferenceType = isReferenceType;
        _isGenericType = isGenericType;
        _isNullable = isNullable;
        _fieldIds = fieldIds;
        _propertyIds = propertyIds;
        _methodIds = methodIds;
        _underlyingTypeId = underlyingTypeId;
        _genericTypeArgumentIds = genericTypeArgumentIds;
    }

    public ulong Id => _id;
    public string Name => _name;
    public string FullName => _fullName;
    public string AssemblyQualifiedName => _assemblyQualifiedName;
    public bool IsValueType => _isValueType;
    public bool IsReferenceType => _isReferenceType;
    public bool IsGenericType => _isGenericType;
    public bool IsNullable => _isNullable;

    public TypeInfoGen? UnderlyingType
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if (_underlyingTypeId == null)
                return null;
            return TypeRegistry.GetType(_underlyingTypeId.Value);
        }
    }

    public TypeInfoGen[] GenericTypeArguments
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if (_genericTypeArgumentIds == null || _genericTypeArgumentIds.Length == 0)
                return Array.Empty<TypeInfoGen>();

            var result = new TypeInfoGen[_genericTypeArgumentIds.Length];
            for (int i = 0; i < result.Length; i++)
                result[i] = TypeRegistry.GetType(_genericTypeArgumentIds[i]);
            return result;
        }
    }

    public PropertyInfoGen[] Properties
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => PropertyRegistry.GetPropertiesForType(_id).ToArray();
    }

    public FieldInfoGen[] Fields
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => FieldRegistry.GetFieldsForType(_id).ToArray();
    }

    public MethodInfoGen[] Methods
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => MethodRegistry.GetMethodsForType(_id).ToArray();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PropertyInfoGen? GetProperty(string name)
    {
        return PropertyRegistry.GetPropertyByName(_id, name);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FieldInfoGen? GetField(string name)
    {
        return FieldRegistry.GetFieldByName(_id, name);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MethodInfoGen? GetMethod(string name)
    {
        return MethodRegistry.GetMethodByName(_id, name);
    }
}

