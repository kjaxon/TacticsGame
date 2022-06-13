﻿using UnityEngine;

namespace RedBjorn.ProtoTiles.Tiles.Conditions
{
    [CreateAssetMenu(menuName = "Red Bjorn/Tiles/Conditions/Not")]
    public class Not : TileCondition
    {
        public TileCondition Condition;

        public override bool IsMet(TileEntity tile)
        {
            return Condition == null ? false : !Condition.IsMet(tile);
        }
    }
}
