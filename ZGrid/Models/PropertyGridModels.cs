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
    Cascader,
    ListPicker
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

// Tree node contract for cascader data
public interface ITreeNode
{
    IEnumerable<ITreeNode> Children { get; }
}

// Attribute to mark a property to use the Cascader editor
// Optional ItemsSourcePropertyName: name of a property on the owning object
// that provides IEnumerable<ITreeNode> as the root nodes.
[AttributeUsage(AttributeTargets.Property)]
public sealed class CascaderEditorAttribute : Attribute
{
    public string? ItemsSourcePropertyName { get; }
    public CascaderEditorAttribute() { }
    public CascaderEditorAttribute(string itemsSourcePropertyName) => ItemsSourcePropertyName = itemsSourcePropertyName;
}

// Attribute to mark a property to use the generic List<T> picker editor.
// ItemsSourcePropertyName should point to an IEnumerable (preferably IEnumerable<T>)
// available on the same owning object.
[AttributeUsage(AttributeTargets.Property)]
public sealed class ListPickerEditorAttribute : Attribute
{
    public string ItemsSourcePropertyName { get; }
    public ListPickerEditorAttribute(string itemsSourcePropertyName)
        => ItemsSourcePropertyName = itemsSourcePropertyName;
}

// Simple node model for 3-level cascader
public sealed class CascaderNode : ITreeNode
{
    public string Name { get; }
    public IList<CascaderNode> ChildrenList { get; }
    public CascaderNode(string name, IList<CascaderNode>? children = null)
    {
        Name = name;
        ChildrenList = children ?? new List<CascaderNode>();
    }

    IEnumerable<ITreeNode> ITreeNode.Children => ChildrenList;

    public override string ToString() => Name;
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

    public ObservableCollection<ITreeNode> Level1Options { get; } = new();
    public ObservableCollection<ITreeNode> Level2Options { get; } = new();
    public ObservableCollection<ITreeNode> Level3Options { get; } = new();

    private IList<ITreeNode> _cascaderSource = new List<ITreeNode>();

    private ITreeNode? _selectedLevel1;
    public ITreeNode? SelectedLevel1
    {
        get => _selectedLevel1;
        set
        {
            if (!ReferenceEquals(_selectedLevel1, value))
            {
                _selectedLevel1 = value;
                OnPropertyChanged(nameof(SelectedLevel1));
                LoadLevel2();
            }
        }
    }

    private ITreeNode? _selectedLevel2;
    public ITreeNode? SelectedLevel2
    {
        get => _selectedLevel2;
        set
        {
            if (!ReferenceEquals(_selectedLevel2, value))
            {
                _selectedLevel2 = value;
                OnPropertyChanged(nameof(SelectedLevel2));
                LoadLevel3();
            }
        }
    }

    private ITreeNode? _selectedLevel3;
    public ITreeNode? SelectedLevel3
    {
        get => _selectedLevel3;
        set
        {
            if (!ReferenceEquals(_selectedLevel3, value))
            {
                _selectedLevel3 = value;
                OnPropertyChanged(nameof(SelectedLevel3));
                // Commit value when all three are selected
                if (SelectedLevel1 is not null && SelectedLevel2 is not null && SelectedLevel3 is not null)
                {
                    Value = string.Join('/', SelectedLevel1.ToString(), SelectedLevel2.ToString(), SelectedLevel3.ToString());
                    // collapse after selection
                    IsCascaderOpen = false;
                }
            }
        }
    }

    public ICommand ToggleCascaderCommand { get; }

    // Generic List<T> picker state
    public ObservableCollection<object> ItemOptions { get; } = new();

    private object? _selectedItem;
    public object? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (!ReferenceEquals(_selectedItem, value))
            {
                _selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
                OnPropertyChanged(nameof(SelectedItemDisplay));
                if (value is not null)
                {
                    var targetType = _descriptor.PropertyType;
                    object? toSet = value;
                    if (!targetType.IsAssignableFrom(value.GetType()))
                    {
                        // fall back to string conversion
                        var s = value.ToString();
                        if (targetType == typeof(string))
                            toSet = s;
                        else
                        {
                            try
                            {
                                toSet = TypeDescriptor.GetConverter(targetType).ConvertFromString(null, CultureInfo.CurrentCulture, s);
                            }
                            catch
                            {
                                toSet = null;
                            }
                        }
                    }
                    Value = toSet;
                }
                else
                {
                    Value = null;
                }
            }
        }
    }

    public string? SelectedItemDisplay => SelectedItem?.ToString();

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
        else if (type == typeof(string) && descriptor.Attributes.OfType<CascaderEditorAttribute>().FirstOrDefault() is { } cascaderAttr)
        {
            EditorKind = EditorKind.Cascader;
            // Resolve tree source if provided
            if (!string.IsNullOrWhiteSpace(cascaderAttr.ItemsSourcePropertyName))
            {
                var prop = _instance.GetType().GetProperty(cascaderAttr.ItemsSourcePropertyName!);
                if (prop?.GetValue(_instance) is IEnumerable enumerable)
                {
                    var list = new List<ITreeNode>();
                    foreach (var item in enumerable)
                    {
                        if (item is ITreeNode tn) list.Add(tn);
                    }
                    _cascaderSource = list;
                }
            }
            // If no external source is provided, keep empty; UI will show no options by design
            LoadLevel1();
            // Try to preselect from current value using displayed text
            if (_descriptor.GetValue(_instance) is string s && !string.IsNullOrWhiteSpace(s))
            {
                var parts = s.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (parts.Length > 0) SelectedLevel1 = FindByToString(_cascaderSource, parts[0]);
                if (parts.Length > 1 && SelectedLevel1 is not null)
                    SelectedLevel2 = FindByToString(SelectedLevel1.Children, parts[1]);
                if (parts.Length > 2 && SelectedLevel2 is not null)
                    SelectedLevel3 = FindByToString(SelectedLevel2.Children, parts[2]);
            }
        }
        else if (descriptor.Attributes.OfType<ListPickerEditorAttribute>().FirstOrDefault() is { } pickerAttr)
        {
            EditorKind = EditorKind.ListPicker;
            // Resolve items source from owning instance
            var prop = _instance.GetType().GetProperty(pickerAttr.ItemsSourcePropertyName);
            if (prop != null)
            {
                if (prop.GetValue(_instance) is IEnumerable enumerable)
                {
                    foreach (var item in enumerable)
                        ItemOptions.Add(item);
                }
            }
            // Preselect current value
            var current = _descriptor.GetValue(_instance);
            if (current != null)
            {
                var found = ItemOptions.FirstOrDefault(o => ReferenceEquals(o, current))
                            ?? ItemOptions.FirstOrDefault(o => Equals(o, current))
                            ?? ItemOptions.FirstOrDefault(o => string.Equals(o?.ToString(), current.ToString(), StringComparison.CurrentCulture));
                if (found != null) _selectedItem = found;
            }
        }
        else
            EditorKind = EditorKind.Text;

        ToggleCascaderCommand = new SimpleCommand(() => IsCascaderOpen = !IsCascaderOpen);
    }

    private static ITreeNode? FindByToString(IEnumerable<ITreeNode> nodes, string text)
        => nodes.FirstOrDefault(n => string.Equals(n.ToString(), text, StringComparison.CurrentCulture));

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
                OnPropertyChanged(nameof(SelectedItem));
                OnPropertyChanged(nameof(SelectedItemDisplay));
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
            Level1Options.Add(n);
        Level2Options.Clear();
        Level3Options.Clear();
    }

    private void LoadLevel2()
    {
        Level2Options.Clear();
        Level3Options.Clear();
        _selectedLevel2 = null; OnPropertyChanged(nameof(SelectedLevel2));
        _selectedLevel3 = null; OnPropertyChanged(nameof(SelectedLevel3));
        if (SelectedLevel1 is null) return;
        foreach (var n in SelectedLevel1.Children)
            Level2Options.Add(n);
    }

    private void LoadLevel3()
    {
        Level3Options.Clear();
        _selectedLevel3 = null; OnPropertyChanged(nameof(SelectedLevel3));
        if (SelectedLevel1 is null || SelectedLevel2 is null) return;
        foreach (var n in SelectedLevel2.Children)
            Level3Options.Add(n);
    }

    private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
