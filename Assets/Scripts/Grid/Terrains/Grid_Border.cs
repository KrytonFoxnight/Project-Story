using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid_Border : Grid
{
    public override void Init()
    {
        grid_name = "Border_Grid";
        grid_idx = -1;
        grid_movement_cost = 9999;
        grid_vision_cost = 9999;
        grid_type = Grid_Type.Border;
    }
}