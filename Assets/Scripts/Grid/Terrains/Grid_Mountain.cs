using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid_Mountain : Grid
{
    public override void Init()
    {
        grid_name = "Mountain_Grid";
        grid_idx = 3;
        grid_movement_cost = 5;
        grid_vision_cost = 5;
        grid_type = Grid_Type.Terrain;
    }
}
