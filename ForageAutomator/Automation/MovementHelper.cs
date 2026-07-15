using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace ForageAutomator.Automation
{
    internal static class MovementHelper
    {
        // Vanilla max (horse + speed food) stays well below this per game tick.
        private const float ModSpeedTileDistancePerTick = 3f;
        private const int BaseSettleTicks = 4;
        private const int SnapSettleTicks = 2;
        private const int MaxSettleTicks = 20;

        public static bool IsRidingHorse(Farmer player)
        {
            return player.isRidingHorse();
        }

        public static bool ShouldUsePathfinding(ModConfig config, Farmer player, ref Vector2 lastPosition, bool hasLastPosition)
        {
            return config.UsePathfinding && !IsHighSpeedMovement(player, ref lastPosition, hasLastPosition);
        }

        public static bool TrySnapToStandTile(GameLocation location, Farmer player, Vector2 standTile, bool usePathfinding = true)
        {
            if (!usePathfinding)
            {
                Vector2 nearbyStand = ResolveNearbyStandTile(location, standTile);
                if (!WalkabilityHelper.CanFarmerStandOn(location, nearbyStand))
                    return false;

                player.controller = null;
                player.setTileLocation(nearbyStand);
                CollectionHelper.FaceTarget(player, standTile);
                ReleasePlayerControlIfNeeded(player);
                return true;
            }

            HashSet<Vector2> reachable = WalkabilityHelper.GetReachableTiles(location, player.Tile);
            Vector2 resolved = ResolveReachableStandTile(location, player, standTile, reachable);
            if (!reachable.Contains(resolved))
                return false;

            player.controller = null;
            player.setTileLocation(resolved);
            CollectionHelper.FaceTarget(player, standTile);
            ReleasePlayerControlIfNeeded(player);
            return true;
        }

        public static void SnapToStandTile(Farmer player, Vector2 standTile, bool usePathfinding = true)
        {
            GameLocation location = Game1.currentLocation;
            if (!TrySnapToStandTile(location, player, standTile, usePathfinding))
                ReleasePlayerControlIfNeeded(player);
        }

        public static void ReleasePlayerControl(Farmer player)
        {
            player.controller = null;
            player.completelyStopAnimatingOrDoingAction();
            Farmer.canMoveNow(player);
        }

        /// <summary>
        /// Clear automation locks without resetting normal walking animations every tick.
        /// </summary>
        public static void ReleasePlayerControlIfNeeded(Farmer player)
        {
            if (player.controller != null
                || !player.CanMove
                || player.UsingTool
                || player.FarmerSprite.PauseForSingleAnimation)
            {
                ReleasePlayerControl(player);
            }
        }

        public static Vector2 ResolveReachableStandTile(GameLocation location, Farmer player, Vector2 preferredTile)
        {
            HashSet<Vector2> reachable = WalkabilityHelper.GetReachableTiles(location, player.Tile);
            return ResolveReachableStandTile(location, player, preferredTile, reachable);
        }

        private static Vector2 ResolveReachableStandTile(
            GameLocation location,
            Farmer player,
            Vector2 preferredTile,
            HashSet<Vector2> reachable)
        {
            if (WalkabilityHelper.CanFarmerStandOn(location, preferredTile) && reachable.Contains(preferredTile))
                return preferredTile;

            Vector2? best = null;
            float bestDistance = float.MaxValue;

            for (int radius = 1; radius <= 2; radius++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    for (int y = -radius; y <= radius; y++)
                    {
                        if (x == 0 && y == 0)
                            continue;

                        Vector2 candidate = preferredTile + new Vector2(x, y);
                        if (!WalkabilityHelper.CanFarmerStandOn(location, candidate))
                            continue;

                        if (!reachable.Contains(candidate))
                            continue;

                        float distance = Vector2.DistanceSquared(candidate, preferredTile);
                        if (distance >= bestDistance)
                            continue;

                        bestDistance = distance;
                        best = candidate;
                    }
                }

                if (best.HasValue)
                    return best.Value;
            }

            return preferredTile;
        }

        private static Vector2 ResolveNearbyStandTile(GameLocation location, Vector2 preferredTile)
        {
            if (WalkabilityHelper.CanFarmerStandOn(location, preferredTile))
                return preferredTile;

            Vector2? best = null;
            float bestDistance = float.MaxValue;

            for (int radius = 1; radius <= 2; radius++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    for (int y = -radius; y <= radius; y++)
                    {
                        if (x == 0 && y == 0)
                            continue;

                        Vector2 candidate = preferredTile + new Vector2(x, y);
                        if (!WalkabilityHelper.CanFarmerStandOn(location, candidate))
                            continue;

                        float distance = Vector2.DistanceSquared(candidate, preferredTile);
                        if (distance >= bestDistance)
                            continue;

                        bestDistance = distance;
                        best = candidate;
                    }
                }

                if (best.HasValue)
                    return best.Value;
            }

            return preferredTile;
        }

        public static bool IsHighSpeedMovement(Farmer player, ref Vector2 lastPosition, bool hasLastPosition)
        {
            if (player.addedSpeed > 5f)
                return true;

            if (!hasLastPosition)
                return false;

            float movedTiles = Vector2.Distance(player.Position, lastPosition) / Game1.tileSize;
            return movedTiles >= ModSpeedTileDistancePerTick;
        }

        public static bool IsCloseEnoughToCollect(Farmer player, ForageTarget target)
        {
            if (target.Type == ForageType.Bush && target.Source is Bush bush)
                return BushHelper.CanInteractWith(Game1.currentLocation, player, bush);

            if (target.Type == ForageType.Panning)
                return PanningHelper.CanInteractFrom(Game1.currentLocation, player, target.Tile, ToolHelper.FindCopperPan(player));

            return CollectionHelper.CanInteractWith(player, target.Tile);
        }

        public static int GetSettleTicks(bool usedSnap)
        {
            return usedSnap ? SnapSettleTicks : BaseSettleTicks;
        }

        public static int GetMaxSettleTicks()
        {
            return MaxSettleTicks;
        }

        public static Vector2 GetStandOrTargetTile(ForageTarget target)
        {
            if (target.StandTile != Vector2.Zero)
                return target.StandTile;

            if (target.Type == ForageType.Bush && target.Source is Bush bush)
                return bush.Tile;

            if (target.Type == ForageType.Panning)
            {
                Vector2? stand = PanningHelper.ResolveStandTile(Game1.currentLocation, Game1.player, target, target.Tile);
                return stand ?? Game1.player.Tile;
            }

            return target.Tile;
        }
    }

}
