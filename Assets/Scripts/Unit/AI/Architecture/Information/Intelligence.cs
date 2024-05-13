using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Intelligence : IComparable<Intelligence>
{
    private int importance, validity, deterioriation, turn, score;
    public Grid target;
    public Intelligence(int impo, int valid, int deter, int turn, Grid target)
    {
        this.importance = impo;
        this.validity = valid;
        this.deterioriation = deter;
        this.turn = turn;
        this.target = target;
        Calculate_Score();
    }

    public void TurnOver()
    {
        this.validity -= this.deterioriation;
        this.turn--;
        Calculate_Score();
    }

    void Calculate_Score()
    {
        float turn_point;
        if(turn > 3) turn_point = 3.0f;
        else if(turn <= 0) turn_point = 0.75f;
        else turn_point = (float)this.turn;
        
        float temp_score = 100.0f * Mathf.Pow(importance, 1.5f) * Mathf.Pow(validity, 1.3f) * Mathf.Sqrt((float)deterioriation / turn_point);
        this.score = (int)temp_score;
    }

    public int CompareTo(Intelligence other)
    {
        if(other != null) return score.CompareTo(other.score);
        return 1;
    }
}