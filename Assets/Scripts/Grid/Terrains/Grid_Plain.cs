using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid_Plain : Grid
{
    public override void Init()
    {
        grid_name = "Plain_Grid";
        grid_idx = 1;
        grid_movement_cost = 1;
        grid_vision_cost = 1;
        grid_type = Grid_Type.Terrain;
    }
}
