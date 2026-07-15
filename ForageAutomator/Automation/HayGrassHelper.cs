using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace ForageAutomator.Automation
{
    internal static class HayGrassHelper
    {
        public static bool IsHarvestable(GameLocation location, Vector2 tile, Grass grass)
        {
            if (grass.grassType.Value < 1)
                return false;

            return location.terrainFeatures.ContainsKey(tile);
        }

        public static bool HasHayStorage(GameLocation location)
        {
            return location.tryToAddHay(0) == 0;
        }

        public static bool TryGetAt(GameLocation location, Vector2 tile, out Grass? grass)
        {
            if (location.terrainFeatures.TryGetValue(tile, out TerrainFeature? feature) && feature is Grass grassFeature)
            {
                grass = grassFeature;
                return true;
            }

            grass = null;
            return false;
        }
    }

}
