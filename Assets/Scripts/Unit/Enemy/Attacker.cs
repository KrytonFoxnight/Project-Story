using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attacker : Enemy
{
    public override void Init()
    {
        Unit_Id = 2;
        Unit_Name = "Unit_Attacker";
        maxMovingCost = 4;
        type = Unit_Type.Infantry;
        maxHp = 7;
        attackPower = 2;
        armor = 1;
        attackRange = 3;
        maxAttackCnt = 1;
        visionCost = 7;
    }
}
