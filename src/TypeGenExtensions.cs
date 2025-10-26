using System;
using System.Linq;

namespace Soenneker.Gen.Reflection;

/// <summary>
/// Extension methods for compile-time type generation
/// </summary>
public static partial class TypeGenExtensions
{
    /// <summary>
    /// Gets compile-time generated type information for the specified object.
    /// This method will be replaced by the source generator with optimized non-reflection code.
    /// </summary>
    /// <typeparam name="T">The type to get information for</typeparam>
    /// <param name="obj">The object instance</param>
    /// <returns>Generated type information</returns>
    public static TypeInfo GetTypeGen<T>(this T obj)
    {
        return BuildFromType(typeof(T));
    }

    /// <summary>
    /// Gets compile-time generated type information for the specified type.
    /// This method will be replaced by the source generator with optimized non-reflection code.
    /// </summary>
    /// <typeparam name="T">The type to get information for</typeparam>
    /// <returns>Generated type information</returns>
    public static TypeInfo GetTypeGen<T>()
    {
        return BuildFromType(typeof(T));
    }

    private static TypeInfo BuildFromType(Type type)
    {
        string name;
        if (type.IsArray)
        {
            var elem = type.GetElementType();
            name = (elem?.Name ?? "Object") + "[]";
        }
        else if (type.IsGenericType)
        {
            name = type.Name;
        }
        else
        {
            name = type.Name;
        }

        bool isNullable = Nullable.GetUnderlyingType(type) != null;
        string? underlying = isNullable ? Nullable.GetUnderlyingType(type)?.Name : null;
        string[]? genericArgs = type.IsGenericType ? Array.ConvertAll(type.GetGenericArguments(), t => t.Name) : null;

        return new TypeInfo(
            name,
            type.FullName ?? type.Name,
            type.AssemblyQualifiedName ?? type.FullName ?? type.Name,
            type.IsValueType,
            !type.IsValueType,
            type.IsGenericType,
            isNullable,
            Array.Empty<FieldInfo>(),
            Array.Empty<PropertyInfo>(),
            Array.Empty<MethodInfo>(),
            underlying,
            genericArgs);
    }
}

/// <summary>
/// Represents compile-time generated type information
/// </summary>
public readonly struct TypeInfo
{
    private readonly string _name;
    private readonly string _fullName;
    private readonly string _assemblyQualifiedName;
    private readonly bool _isValueType;
    private readonly bool _isReferenceType;
    private readonly bool _isGenericType;
    private readonly bool _isNullable;
    private readonly FieldInfo[] _fields;
    private readonly PropertyInfo[] _properties;
    private readonly MethodInfo[] _methods;
    private readonly string? _underlyingTypeName;
    private readonly string[]? _genericTypeArgumentNames;

    public TypeInfo(string name, string fullName, string assemblyQualifiedName, bool isValueType, bool isReferenceType, bool isGenericType, bool isNullable, FieldInfo[] fields, PropertyInfo[] properties, MethodInfo[] methods, string? underlyingTypeName, string[]? genericTypeArgumentNames)
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

    public TypeInfo? UnderlyingType
    {
        get
        {
            if (_underlyingTypeName == null)
                return null;
            var n = _underlyingTypeName;
            return new TypeInfo(n, n, n, true, false, false, false, Array.Empty<FieldInfo>(), Array.Empty<PropertyInfo>(), Array.Empty<MethodInfo>(), null, null);
        }
    }

    public TypeInfo[] GenericTypeArguments
    {
        get
        {
            if (_genericTypeArgumentNames == null || _genericTypeArgumentNames.Length == 0)
                return Array.Empty<TypeInfo>();
            return Array.ConvertAll(_genericTypeArgumentNames, n => new TypeInfo(n, n, n, true, false, false, false, Array.Empty<FieldInfo>(), Array.Empty<PropertyInfo>(), Array.Empty<MethodInfo>(), null, null));
        }
    }

    public PropertyInfo[] Properties => _properties ?? Array.Empty<PropertyInfo>();
    public FieldInfo[] Fields => _fields ?? Array.Empty<FieldInfo>();
    public MethodInfo[] Methods => _methods ?? Array.Empty<MethodInfo>();

    public PropertyInfo? GetProperty(string name)
    {
        return Properties.FirstOrDefault(p => p.Name == name);
    }

    public FieldInfo? GetField(string name)
    {
        return Fields.FirstOrDefault(f => f.Name == name);
    }

    public MethodInfo? GetMethod(string name)
    {
        return Methods.FirstOrDefault(m => m.Name == name);
    }
}

/// <summary>
/// Represents compile-time generated property information
/// </summary>
public readonly struct PropertyInfo
{
    private readonly string _name;
    private readonly TypeInfo _propertyType;
    private readonly bool _canRead;
    private readonly bool _canWrite;
    private readonly Func<object?, object?>? _getter;
    private readonly Action<object?, object?>? _setter;

    public PropertyInfo(string name, string propertyTypeName, bool canRead, bool canWrite)
    {
        _name = name;
        _propertyType = new TypeInfo(propertyTypeName, propertyTypeName, propertyTypeName, false, true, propertyTypeName.Contains("`"), false, Array.Empty<FieldInfo>(), Array.Empty<PropertyInfo>(), Array.Empty<MethodInfo>(), null, null);
        _canRead = canRead;
        _canWrite = canWrite;
        _getter = null;
        _setter = null;
    }

    public PropertyInfo(string name, TypeInfo propertyType, bool canRead, bool canWrite, Func<object?, object?>? getter, Action<object?, object?>? setter)
    {
        _name = name;
        _propertyType = propertyType;
        _canRead = canRead;
        _canWrite = canWrite;
        _getter = getter;
        _setter = setter;
    }

    public PropertyInfo(string name, string propertyTypeName, bool canRead, bool canWrite, Func<object?, object?>? getter, Action<object?, object?>? setter)
    {
        _name = name;
        _propertyType = new TypeInfo(propertyTypeName, propertyTypeName, propertyTypeName, false, true, propertyTypeName.Contains("`"), false, Array.Empty<FieldInfo>(), Array.Empty<PropertyInfo>(), Array.Empty<MethodInfo>(), null, null);
        _canRead = canRead;
        _canWrite = canWrite;
        _getter = getter;
        _setter = setter;
    }

    public string Name => _name;
    public TypeInfo PropertyType => _propertyType;
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

/// <summary>
/// Represents compile-time generated field information
/// </summary>
public readonly struct FieldInfo
{
    private readonly string _name;
    private readonly string _fieldTypeName;
    private readonly bool _isReadOnly;
    private readonly Func<object?, object?>? _getter;
    private readonly Action<object?, object?>? _setter;

    public FieldInfo(string name, string fieldTypeName, bool isReadOnly)
    {
        _name = name;
        _fieldTypeName = fieldTypeName;
        _isReadOnly = isReadOnly;
        _getter = null;
        _setter = null;
    }

    public FieldInfo(string name, string fieldTypeName, bool isReadOnly, Func<object?, object?>? getter, Action<object?, object?>? setter)
    {
        _name = name;
        _fieldTypeName = fieldTypeName;
        _isReadOnly = isReadOnly;
        _getter = getter;
        _setter = setter;
    }

    public string Name => _name;
    public TypeInfo FieldType => new TypeInfo(_fieldTypeName, _fieldTypeName, _fieldTypeName, false, true, false, false, Array.Empty<FieldInfo>(), Array.Empty<PropertyInfo>(), Array.Empty<MethodInfo>(), null, null);
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

/// <summary>
/// Represents compile-time generated method information
/// </summary>
public readonly struct MethodInfo
{
    private readonly string _name;
    private readonly string _returnTypeName;
    private readonly bool _isStatic;
    private readonly string[]? _parameterTypeNames;

    public MethodInfo(string name, string returnTypeName, bool isStatic)
    {
        _name = name;
        _returnTypeName = returnTypeName;
        _isStatic = isStatic;
        _parameterTypeNames = Array.Empty<string>();
    }

    public MethodInfo(string name, string returnTypeName, bool isStatic, string[] parameterTypeNames)
    {
        _name = name;
        _returnTypeName = returnTypeName;
        _isStatic = isStatic;
        _parameterTypeNames = parameterTypeNames;
    }

    public string Name => _name;
    public TypeInfo ReturnType => new TypeInfo(_returnTypeName, _returnTypeName, _returnTypeName, false, true, false, false, Array.Empty<FieldInfo>(), Array.Empty<PropertyInfo>(), Array.Empty<MethodInfo>(), null, null);
    public bool IsStatic => _isStatic;
    public TypeInfo[] ParameterTypes => _parameterTypeNames == null || _parameterTypeNames.Length == 0
        ? Array.Empty<TypeInfo>()
        : Array.ConvertAll(_parameterTypeNames, n => new TypeInfo(n, n, n, false, true, false, false, Array.Empty<FieldInfo>(), Array.Empty<PropertyInfo>(), Array.Empty<MethodInfo>(), null, null));

    public object? Invoke(object? obj, params object?[] parameters)
    {
        // No runtime invocation without reflection.
        return null;
    }
}

