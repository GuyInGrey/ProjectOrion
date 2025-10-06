using UnityEngine.Tilemaps;
using UnityEngine;

namespace Assets.Scripts
{
    [CreateAssetMenu(menuName = "2D/Tiles/Custom Terrain Rule Tile")]
    public class CustomTerrainRuleTile : RuleTile<CustomTerrainRuleTile.Neighbor>
    {
        public class Neighbor : TilingRuleOutput.Neighbor
        {
            public const int AnyTerrain = 3;
            public const int NoTerrain = 4;
        }

        public override bool RuleMatch(int neighbor, TileBase other)
        {
            return neighbor switch
            {
                Neighbor.AnyTerrain => other != null,
                Neighbor.NoTerrain => other == null,
                _ => base.RuleMatch(neighbor, other),
            };
        }
    }
}
