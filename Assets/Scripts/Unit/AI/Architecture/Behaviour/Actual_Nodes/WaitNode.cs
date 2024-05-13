using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitNode : ActionNode
{
    public float duration = 1.0f;
    float startTime;

    protected override void OnStart()
    {
        startTime = Time.time;
    }

    protected override void OnStop()
    {

    }

    protected override NodeState OnUpdate()
    {
        if(Time.time - startTime > duration) return NodeState.SUCCESS;
        else return NodeState.RUNNING;
    }
}
