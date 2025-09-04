using System;

namespace Z;

[AttributeUsage(AttributeTargets.Property)]
public sealed class CascaderEditorAttribute : Attribute
{
    public string? ItemsSourcePropertyName { get; }
    public CascaderEditorAttribute() { }
    public CascaderEditorAttribute(string itemsSourcePropertyName) => ItemsSourcePropertyName = itemsSourcePropertyName;
}
