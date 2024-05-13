using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Medic : Enemy
{
    public override void Init()
    {
        Unit_Id = 3;
        Unit_Name = "Unit_Medic";
        maxMovingCost = 4;
        type = Unit_Type.Support;
        maxHp = 7;
        attackPower = 2;
        armor = 1;
        attackRange = 3;
        maxAttackCnt = 1;
        visionCost = 7;
    }
}
