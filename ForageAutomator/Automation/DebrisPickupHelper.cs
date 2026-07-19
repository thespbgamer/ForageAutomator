using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;

namespace ForageAutomator.Automation
{
    internal static class DebrisPickupHelper
    {
        private const float DefaultRadiusTiles = 2.5f;
        private const float FruitTreeRadiusTiles = 4.5f;
        private const float FruitTreeScanRadiusTiles = 6f;

        public static float GetPickupRadius(ForageType type)
        {
            return type == ForageType.FruitTree ? FruitTreeRadiusTiles : DefaultRadiusTiles;
        }

        public static int CollectForDebrisDrop(GameLocation location, Farmer player, ForageType type, Vector2 tile)
        {
            float radius = GetPickupRadius(type);
            int collected = CollectNearTile(location, player, tile, radius);
            collected += CollectNearTile(location, player, player.Tile, radius);
            return collected;
        }

        public static bool HasRemainingDebris(GameLocation location, Farmer player, ForageType type, Vector2 tile)
        {
            float radius = type == ForageType.FruitTree ? FruitTreeScanRadiusTiles : GetPickupRadius(type);

            if (HasDebrisNearTile(location, tile, radius))
                return true;

            return type == ForageType.FruitTree && HasDebrisNearTile(location, player.Tile, radius);
        }

        public static bool HasDebrisNearTile(GameLocation location, Vector2 tile, float radiusTiles = DefaultRadiusTiles)
        {
            if (location.debris.Count == 0)
                return false;

            Vector2 center = CollectionHelper.GetTileCenter(tile);
            float radiusPixels = radiusTiles * Game1.tileSize;

            foreach (Debris debris in location.debris)
            {
                if (IsNearPosition(debris, center, radiusPixels))
                    return true;
            }

            return false;
        }

        public static int CollectNearTile(GameLocation location, Farmer player, Vector2 tile, float radiusTiles = DefaultRadiusTiles)
        {
            if (location.debris.Count == 0)
                return 0;

            Vector2 center = CollectionHelper.GetTileCenter(tile);
            float radiusPixels = radiusTiles * Game1.tileSize;
            int collected = 0;
            var toRemove = new List<Debris>();

            foreach (Debris debris in location.debris)
            {
                if (!IsNearPosition(debris, center, radiusPixels))
                    continue;

                if (TryCollectDebris(player, debris))
                {
                    collected++;
                    toRemove.Add(debris);
                }
            }

            foreach (Debris debris in toRemove)
                location.debris.Remove(debris);

            return collected;
        }

        private static bool IsNearPosition(Debris debris, Vector2 center, float radiusPixels)
        {
            if (debris.Chunks.Count == 0)
                return true;

            foreach (Chunk chunk in debris.Chunks)
            {
                Vector2 position = chunk.position.Value;
                if (Vector2.Distance(position, center) <= radiusPixels)
                    return true;
            }

            return false;
        }

        private static bool TryCollectDebris(Farmer player, Debris debris)
        {
            foreach (Chunk chunk in debris.Chunks)
            {
                if (debris.collect(player, chunk))
                    return true;
            }

            Item? item = debris.item;
            if (item == null)
                return false;

            if (player.addItemToInventory(item) != null)
                return false;

            debris.item = null;
            debris.Chunks.Clear();
            return true;
        }
    }

}
