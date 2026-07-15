using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;

namespace ForageAutomator.Automation
{
    internal static class WalkabilityHelper
    {
        private static readonly Vector2[] CardinalOffsets =
        {
            new(0, 1),
            new(0, -1),
            new(1, 0),
            new(-1, 0)
        };

        private static readonly Vector2[] NeighborOffsets =
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

        private static readonly FieldInfo? SuspensionBridgesField =
            typeof(IslandLocation).GetField("suspensionBridges", BindingFlags.Instance | BindingFlags.Public);

        private static GameLocation? cachedReachableLocation;
        private static Vector2 cachedReachableStart;
        private static HashSet<Vector2>? cachedReachableTiles;

        public static void InvalidateReachabilityCache()
        {
            cachedReachableLocation = null;
            cachedReachableStart = Vector2.Zero;
            cachedReachableTiles = null;
        }

        public static HashSet<Vector2> GetReachableTiles(GameLocation location, Vector2 start)
        {
            if (cachedReachableTiles != null
                && cachedReachableLocation == location
                && cachedReachableStart == start)
            {
                return cachedReachableTiles;
            }

            var reachable = ComputeReachableTiles(location, start);
            cachedReachableLocation = location;
            cachedReachableStart = start;
            cachedReachableTiles = reachable;
            return reachable;
        }

        private static HashSet<Vector2> ComputeReachableTiles(GameLocation location, Vector2 start)
        {
            var reachable = new HashSet<Vector2>();
            var queue = new Queue<Vector2>();
            queue.Enqueue(start);
            reachable.Add(start);

            int width = location.map.Layers[0].LayerWidth;
            int height = location.map.Layers[0].LayerHeight;

            while (queue.Count > 0)
            {
                Vector2 current = queue.Dequeue();

                foreach (Vector2 offset in NeighborOffsets)
                {
                    Vector2 next = current + offset;

                    if (next.X < 0 || next.Y < 0 || next.X >= width || next.Y >= height)
                        continue;

                    if (reachable.Contains(next))
                        continue;

                    if (!CanTraverse(location, current, next))
                        continue;

                    reachable.Add(next);
                    queue.Enqueue(next);
                }
            }

            return reachable;
        }

        public static bool IsReachable(GameLocation location, Vector2 from, Vector2 to)
        {
            return GetReachableTiles(location, from).Contains(to);
        }

        public static bool IsWalkable(GameLocation location, Vector2 tile)
        {
            return CanFarmerStandOn(location, tile);
        }

        /// <summary>
        /// Match PathFindController tile checks: inset farmer box + pathfinding collision.
        /// </summary>
        public static bool CanPathfindToTile(GameLocation location, Vector2 tile, Farmer? farmer = null)
        {
            farmer ??= Game1.player;

            if (!location.isTileOnMap(tile))
                return false;

            return !location.isCollidingPosition(
                GetPathfindTileBounds(tile),
                Game1.viewport,
                isFarmer: true,
                damagesFarmer: 0,
                glider: false,
                character: farmer,
                pathfinding: true);
        }

        public static bool CanFarmerStandOn(GameLocation location, Vector2 tile, Farmer? farmer = null)
        {
            return CanPathfindToTile(location, tile, farmer)
                || IsSuspensionBridgeWalkwayTile(location, tile);
        }

        /// <summary>
        /// Bridge floors over water can fail pathfinding collision while still being standable.
        /// </summary>
        public static bool CanFarmerStandOnForPanning(GameLocation location, Vector2 tile, Farmer? farmer = null)
        {
            if (CanFarmerStandOn(location, tile, farmer))
                return true;

            farmer ??= Game1.player;

            if (!location.isTileOnMap(tile))
                return false;

            return !location.isCollidingPosition(
                GetPathfindTileBounds(tile),
                Game1.viewport,
                isFarmer: true,
                damagesFarmer: 0,
                glider: false,
                character: farmer,
                pathfinding: false);
        }

        /// <summary>
        /// Pan stands must be on dry land; the shimmering spot itself is open water.
        /// </summary>
        public static bool IsDryPanStandTile(GameLocation location, Vector2 tile, Farmer? farmer = null)
        {
            if (!location.isTileOnMap(tile))
                return false;

            if (location.isOpenWater((int)tile.X, (int)tile.Y))
                return false;

            return CanFarmerStandOnForPanning(location, tile, farmer);
        }

        public static bool IsReachablePanStand(
            GameLocation location,
            Vector2 stand,
            Vector2 panTile,
            HashSet<Vector2> reachable,
            Farmer? farmer = null)
        {
            if (!IsDryPanStandTile(location, stand, farmer))
                return false;

            return reachable.Contains(stand);
        }

        public static Vector2? FindStandTile(GameLocation location, Vector2 targetTile, HashSet<Vector2> reachable)
        {
            Vector2? best = null;
            float bestDistance = float.MaxValue;

            foreach (Vector2 offset in StandOffsets)
            {
                Vector2 stand = targetTile + offset;

                if (!reachable.Contains(stand))
                    continue;

                if (!CanFarmerStandOn(location, stand))
                    continue;

                float distance = Vector2.DistanceSquared(stand, Game1.player.Tile);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = stand;
                }
            }

            return best;
        }

        public static Vector2? FindNearbyStandTile(GameLocation location, Vector2 targetTile)
        {
            if (IsAdjacentOrOn(Game1.player.Tile, targetTile) && CanFarmerStandOn(location, Game1.player.Tile))
                return Game1.player.Tile;

            Vector2? best = null;
            float bestDistance = float.MaxValue;

            foreach (Vector2 offset in StandOffsets)
            {
                Vector2 stand = targetTile + offset;
                if (!CanFarmerStandOn(location, stand))
                    continue;

                float distance = Vector2.DistanceSquared(stand, Game1.player.Tile);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = stand;
                }
            }

            return best;
        }

        public static bool IsPathable(HashSet<Vector2> reachable, Vector2 standTile)
        {
            return reachable.Contains(standTile);
        }

        public static bool IsAdjacentOrOn(Vector2 playerTile, Vector2 targetTile)
        {
            return Vector2.Distance(playerTile, targetTile) <= 1.5f;
        }

        public static bool IsWithinRadius(Vector2 playerTile, Vector2 targetTile, int radius)
        {
            return Vector2.Distance(playerTile, targetTile) <= radius + 0.5f;
        }

        private static bool CanTraverse(GameLocation location, Vector2 from, Vector2 to)
        {
            if (Vector2.DistanceSquared(from, to) == 1f)
            {
                return CanPathfindToTile(location, to)
                    || CanTraverseSuspensionBridge(location, from, to)
                    || IsDryPanStandTile(location, to);
            }

            if (!IsTraversableTile(location, to))
                return false;

            Vector2 horizontal = new(to.X, from.Y);
            Vector2 vertical = new(from.X, to.Y);
            return IsTraversableTile(location, horizontal)
                || IsTraversableTile(location, vertical);
        }

        private static bool IsTraversableTile(GameLocation location, Vector2 tile)
        {
            return CanPathfindToTile(location, tile)
                || IsDryPanStandTile(location, tile)
                || IsSuspensionBridgeWalkwayTile(location, tile);
        }

        private static Rectangle GetPathfindTileBounds(Vector2 tile)
        {
            int tileSize = Game1.tileSize;
            return new Rectangle(
                (int)tile.X * tileSize + 1,
                (int)tile.Y * tileSize + 1,
                tileSize - 2,
                tileSize - 2);
        }

        private static bool CanTraverseSuspensionBridge(GameLocation location, Vector2 from, Vector2 to)
        {
            if (Vector2.Distance(from, to) != 1f)
                return false;

            foreach (Rectangle bridgeBounds in GetSuspensionBridgeBounds(location))
            {
                bool fromOnWalkway = IsSuspensionBridgeWalkwayTile(bridgeBounds, from);
                bool toOnWalkway = IsSuspensionBridgeWalkwayTile(bridgeBounds, to);

                if (!fromOnWalkway && !toOnWalkway)
                    continue;

                if (!toOnWalkway)
                    return false;

                if (fromOnWalkway)
                    return true;

                return CanPathfindToTile(location, from);
            }

            return false;
        }

        private static bool IsSuspensionBridgeWalkwayTile(GameLocation location, Vector2 tile)
        {
            foreach (Rectangle bridgeBounds in GetSuspensionBridgeBounds(location))
            {
                if (IsSuspensionBridgeWalkwayTile(bridgeBounds, tile))
                    return true;
            }

            return false;
        }

        private static bool IsSuspensionBridgeWalkwayTile(Rectangle bridgeBounds, Vector2 tile)
        {
            int tileX = (int)tile.X;
            int tileY = (int)tile.Y;
            int bridgeTileX = bridgeBounds.X / Game1.tileSize;
            int bridgeTileY = bridgeBounds.Y / Game1.tileSize;
            int bridgeWidthTiles = bridgeBounds.Width / Game1.tileSize;

            if (tileY != bridgeTileY)
                return false;

            return tileX == bridgeTileX - 1
                || (tileX >= bridgeTileX && tileX < bridgeTileX + bridgeWidthTiles)
                || tileX == bridgeTileX + bridgeWidthTiles;
        }

        private static IEnumerable<Rectangle> GetSuspensionBridgeBounds(GameLocation location)
        {
            if (location is not IslandLocation island || SuspensionBridgesField == null)
                yield break;

            if (SuspensionBridgesField.GetValue(island) is not IEnumerable bridges)
                yield break;

            foreach (object? bridge in bridges)
            {
                if (bridge == null)
                    continue;

                FieldInfo? boundsField = bridge.GetType().GetField("bridgeBounds", BindingFlags.Instance | BindingFlags.Public);
                if (boundsField?.GetValue(bridge) is Rectangle bounds)
                    yield return bounds;
            }
        }
    }

}
