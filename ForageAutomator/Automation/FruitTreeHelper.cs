using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace ForageAutomator.Automation
{
    internal static class FruitTreeHelper
    {
        private static readonly Vector2[] StandOffsets =
        {
            new(0, 1),
            new(0, -1),
            new(1, 0),
            new(-1, 0),
            new(1, 1),
            new(1, -1),
            new(-1, 1),
            new(-1, -1)
        };

        public static bool HasAnyFruit(FruitTree tree)
        {
            for (int i = 0; i < tree.fruit.Count; i++)
            {
                if (tree.fruit[i] != null)
                    return true;
            }

            return false;
        }

        public static bool IsHarvestable(FruitTree tree)
        {
            if (tree.stump.Value)
                return false;

            return HasAnyFruit(tree);
        }

        public static bool TryGetAt(GameLocation location, Vector2 tile, out FruitTree? tree)
        {
            if (location.terrainFeatures.TryGetValue(tile, out TerrainFeature? feature) && feature is FruitTree fruitTree)
            {
                tree = fruitTree;
                return true;
            }

            tree = null;
            return false;
        }

        public static Vector2 ResolveStandTile(GameLocation location, Farmer player, Vector2 treeTile)
        {
            if (WalkabilityHelper.IsAdjacentOrOn(player.Tile, treeTile)
                && player.Tile != treeTile
                && WalkabilityHelper.CanFarmerStandOn(location, player.Tile))
            {
                return player.Tile;
            }

            Vector2? best = null;
            float bestDistance = float.MaxValue;

            foreach (Vector2 offset in StandOffsets)
            {
                Vector2 candidate = treeTile + offset;
                if (!WalkabilityHelper.CanFarmerStandOn(location, candidate))
                    continue;

                float distance = Vector2.DistanceSquared(candidate, player.Tile);
                if (distance >= bestDistance)
                    continue;

                bestDistance = distance;
                best = candidate;
            }

            return best ?? player.Tile;
        }
    }

}
