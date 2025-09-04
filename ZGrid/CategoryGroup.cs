using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Z;

public sealed partial class CategoryGroup : ObservableObject
{
    public string Name { get; }
    public IList<PropertyEntry> Items { get; }

    [ObservableProperty]
    private bool isExpanded = true;

    public CategoryGroup(string name, IList<PropertyEntry> items)
    {
        Name = name;
        Items = items;
    }
}
