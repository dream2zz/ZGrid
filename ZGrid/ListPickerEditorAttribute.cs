using System;

namespace Z;

[AttributeUsage(AttributeTargets.Property)]
public sealed class ListPickerEditorAttribute : Attribute
{
    public string ItemsSourcePropertyName { get; }
    public ListPickerEditorAttribute(string itemsSourcePropertyName)
        => ItemsSourcePropertyName = itemsSourcePropertyName;
}
