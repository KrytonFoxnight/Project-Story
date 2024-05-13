using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    protected int Item_ID;
    protected string Item_Name;
    protected Sprite Item_Sprite;

    public delegate void Item_Effect();
    protected Item_Effect item_Effect;

    public virtual void Init()
    {
        
    }

    public void Set_Item_Effects(Item_Effect newEffect)
    {
        item_Effect += newEffect;
    }

    public void Invoke_Item_Effect()
    {
        this.item_Effect();
    }

    public void Item_Destroyed()
    {

    }

    public int Get_ItemID()
    {
        return Item_ID;
    }

    public string Get_ItemName()
    {
        return Item_Name;
    }

    public Sprite Get_ItemSprite()
    {
        return Item_Sprite;
    }
}
