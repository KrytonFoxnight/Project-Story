using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid_Swamp : Grid
{
    public override void Init()
    {
        grid_name = "Swamp_Grid";
        grid_idx = 0;
        grid_movement_cost = 3;
        grid_vision_cost = 1;
        grid_type = Grid_Type.Terrain;
    }
}
