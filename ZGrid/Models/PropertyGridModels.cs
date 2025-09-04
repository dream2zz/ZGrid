using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using Avalonia.Data.Converters;

namespace ZGrid.Models;

public enum EditorKind
{
    Text,
    Bool,
    Enum,
    Cascader
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

// Attribute to mark a property to use the Cascader editor
[AttributeUsage(AttributeTargets.Property)]
public sealed class CascaderEditorAttribute : Attribute { }

// Simple node model for 3-level cascader
public sealed class CascaderNode
{
    public string Name { get; }
    public IList<CascaderNode> Children { get; }
    public CascaderNode(string name, IList<CascaderNode>? children = null)
    {
        Name = name;
        Children = children ?? new List<CascaderNode>();
    }
}

// Provide some sample data for the cascader editor
public static class DefaultCascaderData
{
    public static readonly IList<CascaderNode> Data = new List<CascaderNode>
    {
        new CascaderNode("一级 A", new List<CascaderNode>
        {
            new CascaderNode("二级 A1", new List<CascaderNode>
            {
                new CascaderNode("三级 A1-1"),
                new CascaderNode("三级 A1-2"),
                new CascaderNode("三级 A1-3"),
            }),
            new CascaderNode("二级 A2", new List<CascaderNode>
            {
                new CascaderNode("三级 A2-1"),
                new CascaderNode("三级 A2-2"),
            })
        }),
        new CascaderNode("一级 B", new List<CascaderNode>
        {
            new CascaderNode("二级 B1", new List<CascaderNode>
            {
                new CascaderNode("三级 B1-1"),
                new CascaderNode("三级 B1-2"),
            }),
            new CascaderNode("二级 B2", new List<CascaderNode>
            {
                new CascaderNode("三级 B2-1"),
            })
        }),
        new CascaderNode("一级 C", new List<CascaderNode>
        {
            new CascaderNode("二级 C1", new List<CascaderNode>
            {
                new CascaderNode("三级 C1-1"),
                new CascaderNode("三级 C1-2"),
                new CascaderNode("三级 C1-3"),
                new CascaderNode("三级 C1-4"),
            })
        })
    };
}

// Simple command helper
public sealed class SimpleCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;
    public SimpleCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute; _canExecute = canExecute;
    }
    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;
    public void Execute(object? parameter) => _execute();
    public event EventHandler? CanExecuteChanged { add { } remove { } }
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

    // Cascader state
    public bool IsCascaderOpen
    {
        get => _isCascaderOpen;
        set
        {
            if (_isCascaderOpen != value)
            {
                _isCascaderOpen = value;
                OnPropertyChanged(nameof(IsCascaderOpen));
            }
        }
    }
    private bool _isCascaderOpen;

    public ObservableCollection<string> Level1Options { get; } = new();
    public ObservableCollection<string> Level2Options { get; } = new();
    public ObservableCollection<string> Level3Options { get; } = new();

    private IList<CascaderNode> _cascaderSource = new List<CascaderNode>();

    private string? _selectedLevel1;
    public string? SelectedLevel1
    {
        get => _selectedLevel1;
        set
        {
            if (_selectedLevel1 != value)
            {
                _selectedLevel1 = value;
                OnPropertyChanged(nameof(SelectedLevel1));
                LoadLevel2();
            }
        }
    }

    private string? _selectedLevel2;
    public string? SelectedLevel2
    {
        get => _selectedLevel2;
        set
        {
            if (_selectedLevel2 != value)
            {
                _selectedLevel2 = value;
                OnPropertyChanged(nameof(SelectedLevel2));
                LoadLevel3();
            }
        }
    }

    private string? _selectedLevel3;
    public string? SelectedLevel3
    {
        get => _selectedLevel3;
        set
        {
            if (_selectedLevel3 != value)
            {
                _selectedLevel3 = value;
                OnPropertyChanged(nameof(SelectedLevel3));
                // Commit value when all three are selected
                if (!string.IsNullOrWhiteSpace(SelectedLevel1) &&
                    !string.IsNullOrWhiteSpace(SelectedLevel2) &&
                    !string.IsNullOrWhiteSpace(SelectedLevel3))
                {
                    Value = string.Join('/', SelectedLevel1, SelectedLevel2, SelectedLevel3);
                    // collapse after selection
                    IsCascaderOpen = false;
                }
            }
        }
    }

    public ICommand ToggleCascaderCommand { get; }

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
        else if (type == typeof(string) && descriptor.Attributes.OfType<CascaderEditorAttribute>().Any())
        {
            EditorKind = EditorKind.Cascader;
            _cascaderSource = DefaultCascaderData.Data;
            LoadLevel1();
            // Try to preselect from current value
            if (_descriptor.GetValue(_instance) is string s && !string.IsNullOrWhiteSpace(s))
            {
                var parts = s.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (parts.Length > 0) SelectedLevel1 = parts[0];
                if (parts.Length > 1) SelectedLevel2 = parts[1];
                if (parts.Length > 2) SelectedLevel3 = parts[2];
            }
        }
        else
            EditorKind = EditorKind.Text;

        ToggleCascaderCommand = new SimpleCommand(() => IsCascaderOpen = !IsCascaderOpen);
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
                OnPropertyChanged(nameof(StringValue));
                OnPropertyChanged(nameof(BoolValue));
                OnPropertyChanged(nameof(EnumValue));
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

    private void LoadLevel1()
    {
        Level1Options.Clear();
        foreach (var n in _cascaderSource)
            Level1Options.Add(n.Name);
        Level2Options.Clear();
        Level3Options.Clear();
    }

    private void LoadLevel2()
    {
        Level2Options.Clear();
        Level3Options.Clear();
        _selectedLevel2 = null; OnPropertyChanged(nameof(SelectedLevel2));
        _selectedLevel3 = null; OnPropertyChanged(nameof(SelectedLevel3));
        if (string.IsNullOrWhiteSpace(SelectedLevel1)) return;
        var n1 = _cascaderSource.FirstOrDefault(n => n.Name == SelectedLevel1);
        if (n1 == null) return;
        foreach (var n in n1.Children)
            Level2Options.Add(n.Name);
    }

    private void LoadLevel3()
    {
        Level3Options.Clear();
        _selectedLevel3 = null; OnPropertyChanged(nameof(SelectedLevel3));
        if (string.IsNullOrWhiteSpace(SelectedLevel1) || string.IsNullOrWhiteSpace(SelectedLevel2)) return;
        var n1 = _cascaderSource.FirstOrDefault(n => n.Name == SelectedLevel1);
        var n2 = n1?.Children.FirstOrDefault(n => n.Name == SelectedLevel2);
        if (n2 == null) return;
        foreach (var n in n2.Children)
            Level3Options.Add(n.Name);
    }

    private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
