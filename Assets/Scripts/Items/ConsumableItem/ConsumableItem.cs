using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumableItem : Item
{
    public uint Max_Usage;
    public uint Current_Usage;

    protected bool UseItem()
    {
        if(Current_Usage < 1)
        {
            Debug.Log("Item Run Out!");
            return false;
        }

        else
        {
            Debug.Log("Use this item!");
            Current_Usage--;
            
            return true;
        }
    }
}
