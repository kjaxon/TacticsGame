using UnityEngine;

namespace RedBjorn.ProtoTiles.Tiles.Conditions
{
    [CreateAssetMenu(menuName = "Red Bjorn/Tiles/Conditions/Have tag")]
    public class HaveTag : TileCondition
    {
        public TileTag Tag;

        public override bool IsMet(TileEntity tile)
        {
            return tile.Preset.Tags.Contains(Tag);
        }
    }
}
