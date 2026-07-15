using StardewValley;
using StardewValley.TerrainFeatures;

namespace ForageAutomator.Automation
{
    internal static class FruitTreeHelper
    {
        public static bool IsHarvestable(FruitTree tree)
        {
            if (tree.stump.Value)
                return false;

            return tree.fruit.Count > 0;
        }

        public static bool TryGetAt(GameLocation location, Microsoft.Xna.Framework.Vector2 tile, out FruitTree? tree)
        {
            if (location.terrainFeatures.TryGetValue(tile, out TerrainFeature? feature) && feature is FruitTree fruitTree)
            {
                tree = fruitTree;
                return true;
            }

            tree = null;
            return false;
        }
    }

}
