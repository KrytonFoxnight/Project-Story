using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Synopsis : ScriptableObject
{
    private uint Id;
    private Scenario[] Scenarios;

    public void Init_Synopsis(uint idx, Scenario[] scenarios)
    {
        this.Id = idx;
        this.Scenarios = scenarios;
    }

    public Scenario Get_Scenario(ScenarioType type, uint idx)
    {
        uint target = 0;

        switch(type)
        {
            case ScenarioType.Prologue:
                target = (uint)(Scenarios.Length - 2);
                break;

            case ScenarioType.Intro:
                target = 0;
                break;

            case ScenarioType.Epilogue:
                target = (uint)(Scenarios.Length - 1);
                break;

            case ScenarioType.Stage_Head:
                target = 2 * idx - 1;
                break;

            case ScenarioType.Stage_Tail:
                target = 2 * idx;
                break; 

            default:
                break;
        }

        if(target >= Scenarios.Length) return null;
        return this.Scenarios[target];
    }
}
