using System.Collections.Generic;

namespace Z;

public sealed class CascaderNode : ITreeNode
{
    public string Name { get; }
    public IList<CascaderNode> ChildrenList { get; }
    public CascaderNode(string name, IList<CascaderNode>? children = null)
    {
        Name = name;
        ChildrenList = children ?? new List<CascaderNode>();
    }

    System.Collections.Generic.IEnumerable<ITreeNode> ITreeNode.Children => ChildrenList;

    public override string ToString() => Name;
}
