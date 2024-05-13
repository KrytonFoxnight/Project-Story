using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target_Acquire : DecoratorNode
{
    protected override void OnStart()
    {
        if(DEBUG) Debug.Log("Target_Acquire Start");
    }

    protected override void OnStop()
    {
        if(DEBUG) Debug.Log("Target_Acquire Stop");
    }

    protected override NodeState OnUpdate()
    {
        GameManager game = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
        this.ROOT.target = game.GetTarget();
        if(ROOT.target != null)
        {
            child.NodeUpdate();
            return NodeState.RUNNING;
        }
        else
        {
            if(DEBUG) Debug.Log("Target_Acquire Node Error : tartget is null!");
            return NodeState.SUCCESS;
        }
    }
}
