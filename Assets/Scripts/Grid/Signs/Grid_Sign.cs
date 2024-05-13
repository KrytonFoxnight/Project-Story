using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid_Sign : Grid
{
    public override void Init()
    {
        grid_name = "Sign_Grid";
        grid_idx = 0;
        grid_movement_cost = 1;
        grid_vision_cost = 0;
        grid_type = Grid_Type.Sign;
    }
}
