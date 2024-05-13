using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid_Forest : Grid
{
    public override void Init()
    {
        grid_name = "Forest_Grid";
        grid_idx = 2;
        grid_movement_cost = 2;
        grid_vision_cost = 1;
        grid_type = Grid_Type.Terrain;
    }
}
