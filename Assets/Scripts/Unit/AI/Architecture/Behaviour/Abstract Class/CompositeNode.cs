using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CompositeNode : Node
{
    public List<Node> children = new List<Node>();

    public override Node Clone(Node root_node)
    {
        CompositeNode node = Instantiate(this);
        node.ROOT = root_node as RootNode;
        node.children = children.ConvertAll(c => c.Clone(root_node));
        return node;
    }
}
