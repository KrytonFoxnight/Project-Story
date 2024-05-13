using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepeatorNode : DecoratorNode
{
    public int max_repeat = 0;
    int count = 0;

    protected override void OnStart()
    {
        if(DEBUG) Debug.Log("Repeater is started!");
    }

    protected override void OnStop()
    {
        if(DEBUG) Debug.Log("Repeater is finished!");
    }

    protected override NodeState OnUpdate()
    {
        if(max_repeat > 0)
        {
            if(count < max_repeat)
            {
                child.NodeUpdate();
                count++;
                return NodeState.RUNNING;
            }
            else return NodeState.SUCCESS;
        }
        else
        {
            child.NodeUpdate();
            return NodeState.RUNNING;
        }
    }
}
