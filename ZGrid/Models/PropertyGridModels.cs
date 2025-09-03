using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Avalonia.Data.Converters;

namespace ZGrid.Models;

public enum EditorKind
{
    Text,
    Bool,
    Enum
}

public sealed class EditorKindEqualsConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is EditorKind actual)
        {
            if (parameter is EditorKind expected)
                return actual == expected;
            if (parameter is string s && Enum.TryParse<EditorKind>(s, out var parsed))
                return actual == parsed;
        }
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class NotConverter : IValueConverter
{
    public static readonly NotConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : value is null;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : value is null;
}

public sealed class BoolToGlyphConverter : IValueConverter
{
    // true -> expanded '-', false -> collapsed '+'
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b && b ? "-" : "+";

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string s && s == "-";
}

public sealed class CategoryGroup : INotifyPropertyChanged
{
    public string Name { get; }
    public IList<PropertyEntry> Items { get; }

    private bool _isExpanded = true;
    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (_isExpanded != value)
            {
                _isExpanded = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsExpanded)));
            }
        }
    }

    public CategoryGroup(string name, IList<PropertyEntry> items)
    {
        Name = name;
        Items = items;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}

public sealed class PropertyEntry : INotifyPropertyChanged
{
    private readonly object _instance;
    private readonly PropertyDescriptor _descriptor;

    public string Name => _descriptor.Name;
    public string DisplayName => _descriptor.DisplayName ?? _descriptor.Name;
    public string Description => _descriptor.Description ?? string.Empty;
    public string Category => _descriptor.Category ?? "Misc";
    public string TypeName { get; }
    public bool IsReadOnly => _descriptor.IsReadOnly;

    public EditorKind EditorKind { get; }

    public IEnumerable EnumValues { get; } = Array.Empty<object>();

    public event PropertyChangedEventHandler? PropertyChanged;

    public PropertyEntry(object instance, PropertyDescriptor descriptor)
    {
        _instance = instance;
        _descriptor = descriptor;
        TypeName = GetFriendlyTypeName(descriptor.PropertyType);

        var type = descriptor.PropertyType;

        if (type == typeof(bool))
            EditorKind = EditorKind.Bool;
        else if (type.IsEnum)
        {
            EditorKind = EditorKind.Enum;
            EnumValues = Enum.GetValues(type);
        }
        else
            EditorKind = EditorKind.Text;
    }

    private static string GetFriendlyTypeName(Type t)
    {
        var nt = Nullable.GetUnderlyingType(t);
        if (nt != null)
            return GetFriendlyTypeName(nt) + "?";
        if (t.IsGenericType)
            return t.Name.Split('`')[0];
        return t.Name switch
        {
            nameof(Boolean) => "Boolean",
            nameof(String) => "String",
            nameof(Int32) => "Int32",
            nameof(Int64) => "Int64",
            nameof(Double) => "Double",
            nameof(Single) => "Single",
            _ => t.Name
        };
    }

    private object? Value
    {
        get => _descriptor.GetValue(_instance);
        set
        {
            if (!_descriptor.IsReadOnly)
            {
                _descriptor.SetValue(_instance, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StringValue)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BoolValue)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EnumValue)));
            }
        }
    }

    public string? StringValue
    {
        get => Convert.ToString(Value, CultureInfo.CurrentCulture);
        set
        {
            if (EditorKind == EditorKind.Text)
            {
                var targetType = _descriptor.PropertyType;
                try
                {
                    object? converted = value;
                    if (targetType != typeof(string))
                    {
                        if (string.IsNullOrWhiteSpace(value))
                        {
                            converted = targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
                        }
                        else
                        {
                            converted = TypeDescriptor.GetConverter(targetType).ConvertFromString(null, CultureInfo.CurrentCulture, value);
                        }
                    }
                    Value = converted;
                }
                catch
                {
                    // ignore conversion errors
                }
            }
        }
    }

    public bool BoolValue
    {
        get => Value is bool b && b;
        set { if (EditorKind == EditorKind.Bool) Value = value; }
    }

    public object? EnumValue
    {
        get => Value;
        set { if (EditorKind == EditorKind.Enum) Value = value; }
    }

}
