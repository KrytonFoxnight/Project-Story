using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public void Effect(string[] effect)
    {
        if(effect[1] == "CLEAR")
        {
            foreach(Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }
        }

        else if(effect[1] != "")
        {
            foreach(Transform child in transform)
            {
                child.gameObject.SetActive(child.name == effect[1]);
            }
        }
    }
}
