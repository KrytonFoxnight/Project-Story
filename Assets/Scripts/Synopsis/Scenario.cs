using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scenario : ScriptableObject
{
    private ScenarioType Type;
    private uint Max_Dialogue, Current_Dialogue;
    private Dialogue[] Dialogues;
    private bool isOver;

    public void Init_Scenario(Dialogue[] dialogues, ScenarioType type)
    {
        this.Max_Dialogue = (uint)(dialogues.Length);
        this.Current_Dialogue = 0;
        this.isOver = false;
        this.Dialogues = dialogues;
        this.Type = type;
    }

    public Dialogue Next_Dialogue()
    {
        if(Max_Dialogue <= Current_Dialogue)
        {
            isOver = true;
            return null;
        }
        else return this.Dialogues[this.Current_Dialogue++];
    }

    public void Rollback()
    {
        this.Current_Dialogue = 0;
        this.isOver = false;
    }

    public ScenarioType Get_Type()  { return this.Type;     }
    public uint Get_Current_Dialogue() { return this.Current_Dialogue; }
    public uint Get_Max_Dialogue() { return this.Max_Dialogue; }
    public bool Is_Over()           { return this.isOver;   }
}
