using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Z;

public sealed partial class PropertyEntry : ObservableObject
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

    [ObservableProperty]
    private bool isCascaderOpen;

    public ObservableCollection<ITreeNode> Level1Options { get; } = new();
    public ObservableCollection<ITreeNode> Level2Options { get; } = new();
    public ObservableCollection<ITreeNode> Level3Options { get; } = new();

    private IList<ITreeNode> _cascaderSource = new List<ITreeNode>();

    [ObservableProperty]
    private ITreeNode? selectedLevel1;
    partial void OnSelectedLevel1Changed(ITreeNode? value) => LoadLevel2();

    [ObservableProperty]
    private ITreeNode? selectedLevel2;
    partial void OnSelectedLevel2Changed(ITreeNode? value) => LoadLevel3();

    [ObservableProperty]
    private ITreeNode? selectedLevel3;
    partial void OnSelectedLevel3Changed(ITreeNode? value)
    {
        if (SelectedLevel1 is not null && SelectedLevel2 is not null && SelectedLevel3 is not null)
        {
            Value = string.Join('/', SelectedLevel1.ToString(), SelectedLevel2.ToString(), SelectedLevel3.ToString());
            IsCascaderOpen = false;
        }
    }

    public ObservableCollection<object> ItemOptions { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedItemDisplay))]
    private object? selectedItem;
    partial void OnSelectedItemChanged(object? value)
    {
        if (value is not null)
        {
            var targetType = _descriptor.PropertyType;
            object? toSet = value;
            if (!targetType.IsAssignableFrom(value.GetType()))
            {
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

    public string? SelectedItemDisplay => SelectedItem?.ToString();

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
            LoadLevel1();
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
            var prop = _instance.GetType().GetProperty(pickerAttr.ItemsSourcePropertyName);
            if (prop != null)
            {
                if (prop.GetValue(_instance) is IEnumerable enumerable)
                {
                    foreach (var item in enumerable)
                        ItemOptions.Add(item);
                }
            }
            var current = _descriptor.GetValue(_instance);
            if (current != null)
            {
                var found = ItemOptions.FirstOrDefault(o => ReferenceEquals(o, current))
                            ?? ItemOptions.FirstOrDefault(o => Equals(o, current))
                            ?? ItemOptions.FirstOrDefault(o => string.Equals(o?.ToString(), current.ToString(), StringComparison.CurrentCulture));
                if (found != null) selectedItem = found;
            }
        }
        else
            EditorKind = EditorKind.Text;
    }

    [RelayCommand]
    private void ToggleCascader() => IsCascaderOpen = !IsCascaderOpen;

    private static ITreeNode? FindByToString(System.Collections.Generic.IEnumerable<ITreeNode> nodes, string text)
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
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(StringValue)));
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(BoolValue)));
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(EnumValue)));
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedItem)));
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedItemDisplay)));
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
        SelectedLevel2 = null;
        SelectedLevel3 = null;
        if (SelectedLevel1 is null) return;
        foreach (var n in SelectedLevel1.Children)
            Level2Options.Add(n);
    }

    private void LoadLevel3()
    {
        Level3Options.Clear();
        SelectedLevel3 = null;
        if (SelectedLevel1 is null || SelectedLevel2 is null) return;
        foreach (var n in SelectedLevel2.Children)
            Level3Options.Add(n);
    }
}
