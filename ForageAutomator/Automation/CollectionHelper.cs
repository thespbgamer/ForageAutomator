using Microsoft.Xna.Framework;
using StardewValley;

namespace ForageAutomator.Automation
{
    internal static class CollectionHelper
    {
        private const float InteractionRangePixels = Game1.tileSize * 1.25f;

        public static bool IsWithinPickupRange(Farmer player, Vector2 targetTile, int radiusTiles)
        {
            Vector2 targetCenter = GetTileCenter(targetTile);
            float maxDistance = (radiusTiles + 0.75f) * Game1.tileSize;
            return Vector2.Distance(player.Position, targetCenter) <= maxDistance;
        }

        public static bool CanInteractWith(Farmer player, Vector2 targetTile)
        {
            if (WalkabilityHelper.IsAdjacentOrOn(player.Tile, targetTile))
                return true;

            return Vector2.Distance(player.Position, GetTileCenter(targetTile)) <= InteractionRangePixels;
        }

        public static void PreparePlayer(Farmer player, Vector2 targetTile)
        {
            player.Halt();
            player.controller = null;
            FaceTarget(player, targetTile);
        }

        public static void PreparePlayerForTarget(GameLocation location, Farmer player, ForageTarget target)
        {
            if (target.Type == ForageType.Panning)
            {
                PanningHelper.PrepareForPanning(player, target.Tile);
                return;
            }

            PreparePlayer(player, BushHelper.GetTargetFaceTile(location, player, target));
        }

        public static void FaceTarget(Farmer player, Vector2 targetTile)
        {
            Vector2 diff = targetTile - player.Tile;

            if (System.Math.Abs(diff.X) > System.Math.Abs(diff.Y))
                player.faceDirection(diff.X > 0 ? 1 : 3);
            else if (diff.Y != 0)
                player.faceDirection(diff.Y > 0 ? 2 : 0);
        }

        public static Vector2 GetTileCenter(Vector2 tile)
        {
            return tile * Game1.tileSize + new Vector2(Game1.tileSize / 2f, Game1.tileSize / 2f);
        }
    }

}
