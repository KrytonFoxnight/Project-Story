using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    abstract class는 instance로 생성할 수 없다.
*/
public class DebugNode : ActionNode
{
    public string message;
    public bool start = true, stop = true, update = true;
    protected override void OnStart()
    {
        if(start) Debug.Log("OnStart() with message : " + message);
    }

    protected override void OnStop()
    {
        if(stop) Debug.Log("OnStop() with message : " + message);
    }

    protected override Node.NodeState OnUpdate()
    {
        if(update) Debug.Log("OnUpdate() with message : " + message);
        return NodeState.SUCCESS;
    }
}