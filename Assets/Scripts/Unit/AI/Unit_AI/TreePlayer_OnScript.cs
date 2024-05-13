using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreePlayer_OnScript : MonoBehaviour
{
    BehaviourTree tree;

    void Start()
    {
        string[] buf = new string[3] {"hello","kryton","foxnight!"};
        tree = ScriptableObject.CreateInstance<BehaviourTree>();
        
        // Node Generation
        // RootNode Example
        RootNode root = ScriptableObject.CreateInstance<RootNode>();

        // DebugNode Example
        DebugNode debug = ScriptableObject.CreateInstance<DebugNode>();
        debug.message = "Test Tree Architecture";

        // RepeatorNode Example
        RepeatorNode repeat = ScriptableObject.CreateInstance<RepeatorNode>();
        repeat.max_repeat = 3;
        repeat.child = debug;
        
        // SequencerNode Example
        SequencerNode sequencer = ScriptableObject.CreateInstance<SequencerNode>();
        for(int i = 0; i < 3; ++i)
        {
            SequencerNode temp_sequencer = ScriptableObject.CreateInstance<SequencerNode>();
            DebugNode temp_debug = ScriptableObject.CreateInstance<DebugNode>();
            WaitNode temp_wait = ScriptableObject.CreateInstance<WaitNode>();

            temp_debug.message = buf[i];
            temp_wait.duration = 0.5f;

            temp_sequencer.children.Add(temp_debug);
            temp_sequencer.children.Add(temp_wait);

            sequencer.children.Add(temp_sequencer);
        }

        // Setting Root Node
        root.child = sequencer;
        tree.root = root;
    }

    void Update()
    {
        if(tree.root != null) tree.TreeUpdate();
    }
}
