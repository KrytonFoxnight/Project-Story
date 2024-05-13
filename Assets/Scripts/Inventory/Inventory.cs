using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Inventory_Type
{
    ConsumableItems,
    Armors
}

public class Inventory : ScriptableObject
{
    public string Inventory_Name;
    public Inventory_Type Type;
    public int Inventory_Size;
    public List<Item> Inventory_ItemsList;
}
