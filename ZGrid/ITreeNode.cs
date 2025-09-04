namespace Z;

public interface ITreeNode
{
    System.Collections.Generic.IEnumerable<ITreeNode> Children { get; }
}
