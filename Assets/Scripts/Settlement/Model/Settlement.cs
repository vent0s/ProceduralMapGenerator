using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using septim.map;

namespace septim.settlement
{
    public class Settlement
    {
        public int settlementId { get; private set; }

        public Tile settlementTile { get; private set; }

        public int[] settlementContinent { get; private set; }

        public bool[] isCapital { get; private set; }

        public Settlement(int settlementId)
        {
            this.settlementId = settlementId;
        }

        public Settlement(int settlementId, Tile settlementTile, int settlementContinent, bool isCapital)
        {
            this.settlementId = settlementId;
            this.settlementTile = settlementTile;
            this.settlementContinent = new int[] { settlementContinent };
            this.isCapital = new bool[] { isCapital };
        }

    }
}

