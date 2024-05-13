using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreePlayer : MonoBehaviour
{
    public BehaviourTree tree;

    void Start()
    {
        Enemy enemy = this.gameObject.GetComponent<Enemy>();
        tree = tree.Clone(enemy);
    }

    void Update()
    {
        tree.TreeUpdate();
    }
}
