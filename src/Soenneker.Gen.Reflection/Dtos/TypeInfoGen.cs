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

    /// <summary>
    /// Gets or sets id.
    /// </summary>
    public ulong Id => _id;
    /// <summary>
    /// Gets or sets name.
    /// </summary>
    public string Name => _name;
    /// <summary>
    /// Gets or sets full name.
    /// </summary>
    public string FullName => _fullName;
    /// <summary>
    /// Gets or sets assembly qualified name.
    /// </summary>
    public string AssemblyQualifiedName => _assemblyQualifiedName;
    /// <summary>
    /// Gets or sets a value indicating whether the instance is value type.
    /// </summary>
    public bool IsValueType => _isValueType;
    /// <summary>
    /// Gets or sets a value indicating whether the instance is reference type.
    /// </summary>
    public bool IsReferenceType => _isReferenceType;
    /// <summary>
    /// Gets or sets a value indicating whether the instance is generic type.
    /// </summary>
    public bool IsGenericType => _isGenericType;
    /// <summary>
    /// Gets or sets a value indicating whether the instance is nullable.
    /// </summary>
    public bool IsNullable => _isNullable;

    /// <summary>
    /// Gets underlying type.
    /// </summary>
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

    /// <summary>
    /// Gets generic type arguments.
    /// </summary>
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

    /// <summary>
    /// Gets properties.
    /// </summary>
    public PropertyInfoGen[] Properties
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => PropertyRegistry.GetPropertiesForType(_id).ToArray();
    }

    /// <summary>
    /// Gets fields.
    /// </summary>
    public FieldInfoGen[] Fields
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => FieldRegistry.GetFieldsForType(_id).ToArray();
    }

    /// <summary>
    /// Gets methods.
    /// </summary>
    public MethodInfoGen[] Methods
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => MethodRegistry.GetMethodsForType(_id).ToArray();
    }

    /// <summary>
    /// Gets property.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns>The result of the operation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PropertyInfoGen? GetProperty(string name)
    {
        return PropertyRegistry.GetPropertyByName(_id, name);
    }

    /// <summary>
    /// Gets field.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns>The result of the operation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FieldInfoGen? GetField(string name)
    {
        return FieldRegistry.GetFieldByName(_id, name);
    }

    /// <summary>
    /// Gets method.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns>The result of the operation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MethodInfoGen? GetMethod(string name)
    {
        return MethodRegistry.GetMethodByName(_id, name);
    }
}

