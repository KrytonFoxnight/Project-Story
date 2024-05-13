using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootNode : Node
{
    public Node child;
    public Enemy enemy;
    public Grid target;

    protected override void OnStart()
    {

    }

    protected override void OnStop()
    {

    }

    protected override NodeState OnUpdate()
    {
        if(child != null) return child.NodeUpdate();
        return NodeState.FAILURE;
    }

    public override Node Clone(Node root_node)
    {
        RootNode node = Instantiate(this);
        node.ROOT = node;
        if(child != null) node.child = child.Clone(node);
        return node;
    }
}
