using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ZGrid.Models;

namespace ZGrid.Controls;

public partial class PropertyGrid : UserControl
{
    public static readonly StyledProperty<object?> SelectedObjectProperty =
        AvaloniaProperty.Register<PropertyGrid, object?>(nameof(SelectedObject));

    public static readonly StyledProperty<PropertyEntry?> SelectedEntryProperty =
        AvaloniaProperty.Register<PropertyGrid, PropertyEntry?>(nameof(SelectedEntry));

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

    public ICommand ToggleGroupCommand { get; }

    public PropertyGrid()
    {
        InitializeComponent();
        this.PropertyChanged += OnPropertyChanged;
        this.AttachedToVisualTree += (_, __) => BuildGroups();
        ToggleGroupCommand = new RelayCommand<CategoryGroup>(g => g.IsExpanded = !g.IsExpanded);
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

    private sealed class RelayCommand<T> : ICommand where T : class
    {
        private readonly Action<T> _execute;
        public RelayCommand(Action<T> execute) => _execute = execute;
        public bool CanExecute(object? parameter) => parameter is T;
        public void Execute(object? parameter)
        {
            if (parameter is T t) _execute(t);
        }
        public event EventHandler? CanExecuteChanged { add { } remove { } }
    }
}
