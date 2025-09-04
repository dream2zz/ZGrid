using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Input;

namespace Z;

public partial class ZGrid : UserControl
{
    public static readonly StyledProperty<object?> SelectedObjectProperty =
        AvaloniaProperty.Register<global::Z.ZGrid, object?>(nameof(SelectedObject));

    public static readonly StyledProperty<PropertyEntry?> SelectedEntryProperty =
        AvaloniaProperty.Register<global::Z.ZGrid, PropertyEntry?>(nameof(SelectedEntry));

    public object? SelectedObject
    {
        get => GetValue(SelectedObjectProperty);
        set => SetValue(SelectedObjectProperty, value);
    }

    public PropertyEntry? SelectedEntry
    {
        get => GetValue(SelectedEntryProperty);
        set => SetValue(SelectedEntryProperty, value);
    }

    public ObservableCollection<CategoryGroup> Groups { get; } = new();

    public ZGrid()
    {
        InitializeComponent();
        this.PropertyChanged += OnPropertyChanged;
        this.AttachedToVisualTree += (_, __) => BuildGroups();
    }

    private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == SelectedObjectProperty)
            BuildGroups();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void BuildGroups()
    {
        Groups.Clear();
        SelectedEntry = null;

        if (SelectedObject is null)
            return;

        var props = TypeDescriptor.GetProperties(SelectedObject)
            .Cast<PropertyDescriptor>()
            .Where(p => p.IsBrowsable)
            .ToList();

        var entries = props.Select(p => new PropertyEntry(SelectedObject!, p)).ToList();

        var grouped = entries
            .GroupBy(e => e.Category)
            .OrderBy(g => g.Key, StringComparer.CurrentCultureIgnoreCase)
            .Select(g => new CategoryGroup(g.Key, g.OrderBy(e => e.DisplayName, StringComparer.CurrentCultureIgnoreCase).ToList()))
            .ToList();

        foreach (var g in grouped)
            Groups.Add(g);
    }

    [RelayCommand]
    private void ToggleGroup(CategoryGroup g) => g.IsExpanded = !g.IsExpanded;
}
