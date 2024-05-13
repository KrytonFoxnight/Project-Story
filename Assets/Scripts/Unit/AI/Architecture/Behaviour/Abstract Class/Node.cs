using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Node : ScriptableObject
{
    public enum NodeState
    {
        RUNNING,
        FAILURE,
        SUCCESS
    }

    [SerializeField] protected RootNode ROOT = null;
    public bool DEBUG = false;
    public NodeState state = NodeState.RUNNING;

    [HideInInspector] public bool started = false;
    // For Behaviour Tree Editor UI
    [HideInInspector] public string guid;
    [HideInInspector] public Vector2 position;
    [TextArea] public string description;

    

    public NodeState NodeUpdate()
    {
        if(!started)
        {
            OnStart();
            started = true;
        }

        state = OnUpdate();
        
        if(state == NodeState.SUCCESS || state == NodeState.FAILURE)
        {
            OnStop();
            started = false;
        }

        return state;
    }

    public virtual Node Clone(Node root_node)
    {
        Node new_node = Instantiate(this);
        new_node.ROOT = root_node as RootNode;
        return new_node;
    }

    protected abstract void OnStart();
    protected abstract void OnStop();
    protected abstract NodeState OnUpdate();
}
