using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using septim.map;

namespace septim.unit
{
    public class Unit
    {
        Tile curTile;
        int[] movabilityMax;    //最大行动点数
        int[] movability;       //当前行动点数
        bool[] canMove;
        
    }
}
