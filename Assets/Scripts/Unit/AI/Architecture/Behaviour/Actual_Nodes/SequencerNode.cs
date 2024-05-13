using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SequencerNode : CompositeNode
{
    int node_idx;
    protected override void OnStart()
    {
        node_idx = 0;
    }

    protected override void OnStop()
    {

    }

    protected override NodeState OnUpdate()
    {
        Node child = children[node_idx];
        switch(child.NodeUpdate())
        {
            case NodeState.SUCCESS:
                node_idx++;
                break;

            case NodeState.FAILURE:
                if(DEBUG) Debug.Log("Sequencer Node : return NodeState.FAILURE at child " + node_idx.ToString());
                return child.state;

            case NodeState.RUNNING:
                return child.state;
        }
        return node_idx == children.Count ? NodeState.SUCCESS : NodeState.RUNNING;
    }
}
