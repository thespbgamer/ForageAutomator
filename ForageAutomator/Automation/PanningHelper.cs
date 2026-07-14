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

            Vector2? best = null;
            float bestDistance = float.MaxValue;

            foreach (Vector2 stand in EnumerateStandCandidates(location, panTile, reach))
            {
                if (!IsValidPanStand(location, stand, panTile, player, pan))
                    continue;

                float distance = Vector2.DistanceSquared(stand, player.Tile);
                if (distance >= bestDistance)
                    continue;

                bestDistance = distance;
                best = stand;
            }

            return best;
        }

        public static Rectangle GetPanInteractionArea(Point panPoint, int reach = BasePanReachTiles)
        {
            int tileSize = Game1.tileSize;

            // Match vanilla Pan.beginUsing: tile offset -1, size reach tiles.
            return new Rectangle(
                panPoint.X * tileSize - tileSize,
                panPoint.Y * tileSize - tileSize,
                tileSize * reach,
                tileSize * reach);
        }

        public static int GetMaxStandingDistance(int reach = BasePanReachTiles)
        {
            // Vanilla allows standing up to 3 tiles from pan-area center with reach 4.
            return (reach - 1) * Game1.tileSize;
        }

        public static bool CanInteractFrom(GameLocation location, Farmer player, Vector2 panTile, Pan? pan = null)
        {
            if (!WalkabilityHelper.IsDryPanStandTile(location, player.Tile, player))
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

        public static bool IsValidPanStand(
            GameLocation location,
            Vector2 standTile,
            Vector2 panTile,
            Farmer player,
            Pan? pan = null)
        {
            pan ??= ToolHelper.FindCopperPan(player);
            return WalkabilityHelper.IsDryPanStandTile(location, standTile, player)
                && CanReachPanFromStand(location, standTile, ToPanPoint(panTile), GetPanReach(pan), panTile);
        }

        public static bool CanReachPanFromStand(
            GameLocation location,
            Vector2 standTile,
            Point panPoint,
            int reach = BasePanReachTiles,
            Vector2? panTile = null)
        {
            if (location.isOpenWater((int)standTile.X, (int)standTile.Y))
                return false;

            Vector2 panVector = panTile ?? panPoint.ToVector2();
            Rectangle panArea = GetPanInteractionArea(panPoint, reach);
            Rectangle standBounds = GetFarmerBoundsFacingPan(standTile, panVector);

            if (standBounds.Intersects(panArea))
                return true;

            Point standingPixel = GetStandingPixelAtTile(standTile);
            Point center = panArea.Center;
            return Utility.distance(
                standingPixel.X,
                center.X,
                standingPixel.Y,
                center.Y) <= GetMaxStandingDistance(reach);
        }

        public static Vector2? FindStandTile(
            GameLocation location,
            Farmer player,
            Vector2 panTile,
            HashSet<Vector2> reachable,
            Pan? pan = null)
        {
            pan ??= ToolHelper.FindCopperPan(player);
            Point panPoint = ToPanPoint(panTile);
            int reach = GetPanReach(pan);

            if (IsValidPanStand(location, player.Tile, panTile, player, pan))
                return player.Tile;

            Vector2? best = null;
            float bestDistance = float.MaxValue;

            foreach (Vector2 stand in reachable)
            {
                if (!IsValidPanStand(location, stand, panTile, player, pan))
                    continue;

                float distance = Vector2.DistanceSquared(stand, player.Tile);
                if (distance >= bestDistance)
                    continue;

                bestDistance = distance;
                best = stand;
            }

            return best ?? FindStandTileAnywhere(location, player, panTile, pan);
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
            if (!IsValidPanStand(location, standTile, panTile, player))
                return false;

            HashSet<Vector2> reachable = WalkabilityHelper.GetReachableTiles(location, player.Tile);
            if (!reachable.Contains(standTile))
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
                && IsValidPanStand(location, target.StandTile, panTile, player, pan))
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

        public static bool IsWithinPassivePickupRange(
            GameLocation location,
            Farmer player,
            ForageTarget target,
            int radiusTiles)
        {
            if (CanInteractFrom(location, player, target.Tile, ToolHelper.FindCopperPan(player)))
                return true;

            if (target.StandTile != Vector2.Zero
                && WalkabilityHelper.IsWithinRadius(player.Tile, target.StandTile, radiusTiles))
            {
                return true;
            }

            return WalkabilityHelper.IsWithinRadius(
                player.Tile,
                target.Tile,
                radiusTiles + GetPanReach(ToolHelper.FindCopperPan(player)));
        }

        public static bool IsReadyToCollect(GameLocation location, Farmer player, ForageTarget target)
        {
            return CanInteractFrom(location, player, target.Tile, ToolHelper.FindCopperPan(player));
        }

        public static void ClearPanAnimationState(Farmer player)
        {
            ToolAnimationHelper.Cancel(player);
        }

        public static bool TryExecutePan(GameLocation location, Farmer player, Pan pan, Vector2 panTile)
        {
            if (!IsActivePanTile(location, panTile))
                return false;

            if (pan.UpgradeLevel <= 0)
                pan.UpgradeLevel = 1;

            PrepareForPanning(player, panTile);
            ToolHelper.ExecuteToolFunction(location, player, pan);

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

            return !IsActivePanTile(location, panTile);
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

            Point center = panArea.Center;
            return Utility.distance(
                standingPixel.X,
                center.X,
                standingPixel.Y,
                center.Y) <= GetMaxStandingDistance(reach);
        }

        private static Rectangle GetFarmerBoundsFacingPan(Vector2 standTile, Vector2 panTile)
        {
            int tileSize = Game1.tileSize;
            int x = (int)standTile.X * tileSize + 8;
            int y = (int)standTile.Y * tileSize + 16;
            int width = 16;
            int height = 24;

            Vector2 diff = panTile - standTile;
            if (System.Math.Abs(diff.X) > System.Math.Abs(diff.Y))
            {
                if (diff.X > 0)
                    width += tileSize / 2;
                else
                {
                    x -= tileSize / 2;
                    width += tileSize / 2;
                }
            }
            else if (diff.Y > 0)
            {
                height += tileSize / 2;
            }
            else if (diff.Y < 0)
            {
                y -= tileSize / 2;
                height += tileSize / 2;
            }

            return new Rectangle(x, y, width, height);
        }

        private static Point GetStandingPixelAtTile(Vector2 tile)
        {
            int tileSize = Game1.tileSize;
            return new Point(
                (int)tile.X * tileSize + tileSize / 2,
                (int)tile.Y * tileSize + tileSize - 8);
        }

        private static IEnumerable<Vector2> EnumerateStandCandidates(GameLocation location, Vector2 panTile, int reach)
        {
            int searchRadius = reach + 2;
            var ordered = new List<Vector2>();

            for (int dx = -searchRadius; dx <= searchRadius; dx++)
            {
                for (int dy = -searchRadius; dy <= searchRadius; dy++)
                {
                    Vector2 tile = panTile + new Vector2(dx, dy);
                    if (location.isOpenWater((int)tile.X, (int)tile.Y))
                        continue;

                    ordered.Add(tile);
                }
            }

            ordered.Sort((a, b) =>
            {
                bool aShore = IsNearOpenWater(location, a);
                bool bShore = IsNearOpenWater(location, b);
                if (aShore != bShore)
                    return aShore ? -1 : 1;

                float aDistance = Vector2.DistanceSquared(a, panTile);
                float bDistance = Vector2.DistanceSquared(b, panTile);
                return aDistance.CompareTo(bDistance);
            });

            foreach (Vector2 tile in ordered)
                yield return tile;
        }

        private static bool IsNearOpenWater(GameLocation location, Vector2 tile)
        {
            if (!location.isTileOnMap(tile))
                return false;

            int x = (int)tile.X;
            int y = (int)tile.Y;

            if (location.isOpenWater(x, y))
                return false;

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0)
                        continue;

                    if (location.isOpenWater(x + dx, y + dy))
                        return true;
                }
            }

            return false;
        }
        private static Point ToPanPoint(Vector2 panTile)
        {
            return new Point((int)panTile.X, (int)panTile.Y);
        }
    }

}
