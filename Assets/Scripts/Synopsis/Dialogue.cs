using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dialogue : ScriptableObject
{
    private string Char_Name, Text, Position;
    private string[] Event_Animation;
    private string[] Event_Sound;
    private string[] Event_Effect;
    Sprite Char_Sprite;


    public void Init_Dialogue(string char_name, Sprite sprite, string txt, string pos)
    {
        this.Char_Name = char_name;
        this.Char_Sprite = sprite;
        this.Text = txt;
        this.Position = pos;
    }

    public void Set_Value(out string name, out string text, out Sprite sprite, out string pos)
    {
        name = this.Char_Name;
        text = this.Text;
        sprite = this.Char_Sprite;
        pos = this.Position;
    }

    public void Set_Animation(string animation_name, string animation_param, string animation_timing)
    {
        this.Event_Animation = new string[3];
        this.Event_Animation[0] = animation_name;
        this.Event_Animation[1] = animation_param;
        this.Event_Animation[2] = animation_timing;
    }

    public void Set_Sound(string sound_name, string sound_timing)
    {
        this.Event_Sound = new string[2];
        this.Event_Sound[0] = sound_name;
        this.Event_Sound[1] = sound_timing;
    }

    public void Set_Effect(string effect_code, string effect_name, string effect_timing)
    {
        this.Event_Effect = new string[3];
        this.Event_Effect[0] = effect_code;
        this.Event_Effect[1] = effect_name;
        this.Event_Effect[2] = effect_timing;
    }

    public void Print()
    {
        if(this.Char_Name != "") Debug.Log("Char_Name : " + this.Char_Name);
        if(this.Text != "") Debug.Log("Text : " + this.Text);
        if(this.Position != "") Debug.Log("Position : " + this.Position);
        if(this.Char_Sprite != null) Debug.Log("Sprite is Setted.");
    }

    public string Get_Pos()         { return this.Position;         }
    public string Get_Text()        { return this.Text;             }
    public string[] Get_Animation() { return this.Event_Animation;  }
    public string[] Get_Sound()     { return this.Event_Sound;      }
    public string[] Get_Effect()    { return this.Event_Effect;     }
}
