using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Enchantments;
using StardewValley.Tools;

namespace ForageAutomator.Automation
{
    internal static class PanningHelper
    {
        private const int BasePanReachTiles = 4;

        public static int GetPanReach(Pan? pan)
        {
            int reach = BasePanReachTiles;
            if (pan?.hasEnchantmentOfType<ReachingToolEnchantment>() == true)
                reach++;

            return reach;
        }

        public static bool TryGetPanTile(GameLocation location, out Vector2 panTile)
        {
            Point panPoint = location.orePanPoint?.Value ?? Point.Zero;
            if (panPoint == Point.Zero)
            {
                panTile = Vector2.Zero;
                return false;
            }

            panTile = panPoint.ToVector2();
            return true;
        }

        public static bool IsActivePanTile(GameLocation location, Vector2 tile)
        {
            Point current = location.orePanPoint?.Value ?? Point.Zero;
            if (current == Point.Zero)
                return false;

            return current.X == (int)tile.X && current.Y == (int)tile.Y;
        }

        public static Vector2? FindStandTileAnywhere(GameLocation location, Farmer player, Vector2 panTile, Pan? pan = null)
        {
            pan ??= ToolHelper.FindCopperPan(player);
            Point panPoint = ToPanPoint(panTile);
            int reach = GetPanReach(pan);

            foreach (Vector2 stand in EnumerateStandCandidates(panTile, reach))
            {
                if (!WalkabilityHelper.CanFarmerStandOnForPanning(location, stand, player))
                    continue;

                if (!CanReachPanFromStand(stand, panPoint, reach))
                    continue;

                return stand;
            }

            return null;
        }

        public static Rectangle GetPanInteractionArea(Point panPoint, int reach = BasePanReachTiles)
        {
            int tileSize = Game1.tileSize;
            int halfReach = tileSize * reach / 2;
            int fullReach = tileSize * reach;

            return new Rectangle(
                panPoint.X * tileSize - halfReach,
                panPoint.Y * tileSize - halfReach,
                fullReach,
                fullReach);
        }

        public static bool CanInteractFrom(GameLocation location, Farmer player, Vector2 panTile, Pan? pan = null)
        {
            if (!WalkabilityHelper.CanFarmerStandOnForPanning(location, player.Tile, player))
                return false;

            Point panPoint = ToPanPoint(panTile);
            if ((location.orePanPoint?.Value ?? Point.Zero) != panPoint)
                return false;

            return CanInteractFromPosition(
                player.GetBoundingBox(),
                player.StandingPixel,
                panPoint,
                GetPanReach(pan));
        }

        public static bool CanReachPanFromStand(Vector2 standTile, Point panPoint, int reach = BasePanReachTiles)
        {
            Rectangle panArea = GetPanInteractionArea(panPoint, reach);
            int tileSize = Game1.tileSize;
            Rectangle tileBounds = new Rectangle(
                (int)standTile.X * tileSize,
                (int)standTile.Y * tileSize,
                tileSize,
                tileSize);

            if (tileBounds.Intersects(panArea))
                return true;

            Rectangle standBounds = GetFarmerBoundsAtTile(standTile);
            if (standBounds.Intersects(panArea))
                return true;

            Point standingPixel = GetStandingPixelAtTile(standTile);
            return CanInteractFromPosition(standBounds, standingPixel, panPoint, reach);
        }

        public static Vector2? FindStandTile(
            GameLocation location,
            Farmer player,
            Vector2 panTile,
            HashSet<Vector2> reachable,
            Pan? pan = null)
        {
            Point panPoint = ToPanPoint(panTile);
            int reach = GetPanReach(pan);
            Vector2? best = null;
            float bestDistance = float.MaxValue;

            if (WalkabilityHelper.IsReachablePanStand(location, player.Tile, panTile, reachable, player)
                && WalkabilityHelper.CanFarmerStandOnForPanning(location, player.Tile, player)
                && CanReachPanFromStand(player.Tile, panPoint, reach))
            {
                return player.Tile;
            }

            foreach (Vector2 stand in EnumerateStandCandidates(panTile, reach))
            {
                if (!WalkabilityHelper.IsReachablePanStand(location, stand, panTile, reachable, player))
                    continue;

                if (!WalkabilityHelper.CanFarmerStandOnForPanning(location, stand, player))
                    continue;

                if (!CanReachPanFromStand(stand, panPoint, reach))
                    continue;

                float distance = Vector2.DistanceSquared(stand, player.Tile);
                if (distance >= bestDistance)
                    continue;

                bestDistance = distance;
                best = stand;
            }

            return best;
        }

        public static bool TryMoveToPanStand(GameLocation location, Farmer player, ForageTarget target)
        {
            if (!TryGetPanTile(location, out Vector2 panTile))
                return false;

            Pan? pan = ToolHelper.FindCopperPan(player);

            if (CanInteractFrom(location, player, panTile, pan))
                return true;

            Vector2? stand = ResolveStandTile(location, player, target, panTile, pan);
            if (!stand.HasValue)
                return false;

            if (!TrySnapToStand(location, player, stand.Value, panTile))
                return false;

            return CanInteractFrom(location, player, panTile, pan);
        }

        public static bool TrySnapToStand(GameLocation location, Farmer player, Vector2 standTile, Vector2 panTile)
        {
            HashSet<Vector2> reachable = WalkabilityHelper.GetReachableTiles(location, player.Tile);
            if (!WalkabilityHelper.IsReachablePanStand(location, standTile, panTile, reachable, player))
                return false;

            if (!WalkabilityHelper.CanFarmerStandOnForPanning(location, standTile, player))
                return false;

            player.controller = null;
            player.setTileLocation(standTile);
            PrepareForPanning(player, panTile);
            MovementHelper.ReleasePlayerControlIfNeeded(player);
            return true;
        }

        public static void PrepareForPanning(Farmer player, Vector2 panTile)
        {
            CollectionHelper.PreparePlayer(player, panTile);
            player.faceDirection(2);
        }

        public static Vector2? ResolveStandTile(
            GameLocation location,
            Farmer player,
            ForageTarget target,
            Vector2 panTile,
            Pan? pan = null)
        {
            pan ??= ToolHelper.FindCopperPan(player);

            if (target.StandTile != Vector2.Zero
                && WalkabilityHelper.CanFarmerStandOnForPanning(location, target.StandTile, player)
                && CanReachPanFromStand(target.StandTile, ToPanPoint(panTile), GetPanReach(pan)))
            {
                return target.StandTile;
            }

            HashSet<Vector2> reachable = WalkabilityHelper.GetReachableTiles(location, player.Tile);
            return FindStandTile(location, player, panTile, reachable, pan);
        }

        public static Vector2? ResolveStandTile(
            GameLocation location,
            Farmer player,
            ForageTarget target,
            Vector2 panTile)
        {
            return ResolveStandTile(location, player, target, panTile, ToolHelper.FindCopperPan(player));
        }

        public static bool IsReadyToCollect(GameLocation location, Farmer player, ForageTarget target)
        {
            return CanInteractFrom(location, player, target.Tile, ToolHelper.FindCopperPan(player));
        }

        public static void ClearPanAnimationState(Farmer player)
        {
            player.UsingTool = false;
            player.FarmerSprite.PauseForSingleAnimation = false;
            player.completelyStopAnimatingOrDoingAction();
            Farmer.canMoveNow(player);
        }

        public static bool TryExecutePan(GameLocation location, Farmer player, Pan pan, Vector2 panTile)
        {
            if (!IsActivePanTile(location, panTile))
                return false;

            if (pan.UpgradeLevel <= 0)
                pan.UpgradeLevel = 1;

            pan.lastUser = player;
            ClearPanAnimationState(player);

            IList<Item> items = pan.getPanItems(location, player);
            foreach (Item? item in items)
            {
                if (item == null)
                    continue;

                if (player.addItemToInventory(item) != null)
                    return false;
            }

            location.localSound("coin", panTile * Game1.tileSize);
            location.orePanPoint!.Value = Point.Zero;

            int extraRolls = pan.UpgradeLevel - 1;
            for (int roll = 0; roll < extraRolls; )
            {
                if (location.performOrePanTenMinuteUpdate(Game1.random))
                    break;

                if (Game1.random.NextDouble() >= 0.5)
                    break;

                if (!location.performOrePanTenMinuteUpdate(Game1.random))
                    break;

                if (location is not StardewValley.Locations.IslandNorth)
                    break;

                roll++;
            }

            ClearPanAnimationState(player);
            return true;
        }

        private static bool CanInteractFromPosition(
            Rectangle farmerBounds,
            Point standingPixel,
            Point panPoint,
            int reach)
        {
            Rectangle panArea = GetPanInteractionArea(panPoint, reach);
            if (farmerBounds.Intersects(panArea))
                return true;

            if (!panArea.Contains(standingPixel.X, standingPixel.Y))
                return false;

            Point center = panArea.Center;
            float maxDistance = reach * Game1.tileSize;

            return Utility.distance(standingPixel.X, center.X, standingPixel.Y, center.Y) <= maxDistance;
        }

        private static Rectangle GetFarmerBoundsAtTile(Vector2 tile)
        {
            int tileSize = Game1.tileSize;
            return new Rectangle(
                (int)tile.X * tileSize + 8,
                (int)tile.Y * tileSize + 16,
                16,
                24);
        }

        private static Point GetStandingPixelAtTile(Vector2 tile)
        {
            int tileSize = Game1.tileSize;
            return new Point(
                (int)tile.X * tileSize + tileSize / 2,
                (int)tile.Y * tileSize + tileSize / 2);
        }

        private static Point ToPanPoint(Vector2 panTile)
        {
            return new Point((int)panTile.X, (int)panTile.Y);
        }

        private static IEnumerable<Vector2> EnumerateStandCandidates(Vector2 panTile, int maxTileRadius)
        {
            yield return panTile;

            for (int dx = -maxTileRadius; dx <= maxTileRadius; dx++)
            {
                for (int dy = -maxTileRadius; dy <= maxTileRadius; dy++)
                {
                    if (dx == 0 && dy == 0)
                        continue;

                    yield return panTile + new Vector2(dx, dy);
                }
            }
        }
    }

}
