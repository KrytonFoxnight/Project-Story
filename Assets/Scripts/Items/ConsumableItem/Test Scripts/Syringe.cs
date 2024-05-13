using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Syringe : ConsumableItem
{
    public Syringe()
    {
        this.Item_ID = 1;
        this.Item_Name = "Item_Syringe";
        this.Max_Usage = 3;
        this.Current_Usage = 3;

        Set_Item_Effects(Item_Effect_Syringe);
    }

    public void Item_Effect_Syringe()
    {
        Unit unit = UnitManager.Instance.unitToControl.GetComponent<Unit>();
        if(unit != null && this.UseItem())
        {
            unit.maxHp *= 2;
            Debug.Log("Max Double!");
        }

        else
        {
            Debug.Log("Item Usage Failed!");
        }
    }
}
