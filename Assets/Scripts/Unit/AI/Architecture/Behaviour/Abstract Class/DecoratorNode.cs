using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DecoratorNode : Node
{
    public Node child;

    public override Node Clone(Node root_node)
    {
        DecoratorNode node = Instantiate(this);
        node.ROOT = root_node as RootNode;
        node.child = child.Clone(root_node);
        return node;
    }
}
