using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace ForageAutomator.Automation
{
    internal readonly struct BushInteraction
    {
        public Vector2 StandTile { get; init; }
        public Vector2 FaceTile { get; init; }
        public int Facing { get; init; }
    }

    internal static class BushHelper
    {
        private static readonly Vector2[] CardinalOffsets =
        {
            new(0, 1),
            new(0, -1),
            new(1, 0),
            new(-1, 0)
        };

        public static bool IsHarvestable(Bush bush)
        {
            if (bush.townBush.Value)
                return false;

            if (!bush.isActionable())
                return false;

            if (!bush.readyForHarvest() || !bush.inBloom())
                return false;

            return !string.IsNullOrEmpty(bush.GetShakeOffItem());
        }

        public static bool IsInSeasonWithoutProduce(Bush bush)
        {
            if (bush.townBush.Value)
                return false;

            if (!bush.inBloom() || bush.readyForHarvest())
                return false;

            return !string.IsNullOrEmpty(bush.GetShakeOffItem());
        }

        public static void MarkHarvested(Bush bush)
        {
            bush.tileSheetOffset.Set(0);
            bush.setUpSourceRect();
        }

        public static bool CanInteractWith(GameLocation location, Farmer player, Bush bush)
        {
            return TryGetInteraction(location, player, bush, out _, out _);
        }

        public static bool PrepareForInteraction(GameLocation location, Farmer player, Bush bush)
        {
            if (!TryGetInteraction(location, player, bush, out Vector2 faceTile, out int facing))
                return false;

            CollectionHelper.PreparePlayer(player, faceTile);
            player.faceDirection(facing);
            return true;
        }

        public static bool TrySnapForInteraction(GameLocation location, Farmer player, Bush bush, Vector2 preferredStand)
        {
            foreach (BushInteraction interaction in GetRankedInteractions(location, player, bush, preferredStand))
            {
                if (!MovementHelper.TrySnapToStandTile(location, player, interaction.StandTile))
                    continue;

                CollectionHelper.PreparePlayer(player, interaction.FaceTile);
                player.faceDirection(interaction.Facing);

                if (CanInteractWith(location, player, bush))
                    return true;
            }

            return false;
        }

        public static bool TryGetInteraction(GameLocation location, Farmer player, Bush bush, out Vector2 faceTile, out int facing)
        {
            return TryGetInteractionFromStand(location, player.Tile, bush, out faceTile, out facing);
        }

        public static Vector2? FindStandTile(GameLocation location, Farmer player, Bush bush, HashSet<Vector2> reachable)
        {
            BushInteraction? best = null;
            float bestDistance = float.MaxValue;

            foreach (BushInteraction interaction in GetInteractions(location, bush, reachable))
            {
                float distance = Vector2.DistanceSquared(interaction.StandTile, player.Tile);
                if (distance >= bestDistance)
                    continue;

                bestDistance = distance;
                best = interaction;
            }

            return best?.StandTile;
        }

        public static IEnumerable<Vector2> GetOccupiedTiles(Bush bush)
        {
            return GetOccupiedTileSet(bush);
        }

        public static Vector2 GetTargetFaceTile(GameLocation location, Farmer player, ForageTarget target)
        {
            if (target.Type == ForageType.Bush && target.Source is Bush bush
                && TryGetInteraction(location, player, bush, out Vector2 faceTile, out _))
                return faceTile;

            if (target.Type == ForageType.Bush && target.Source is Bush fallbackBush)
                return GetClosestOccupiedTile(player.Tile, fallbackBush);

            return target.Tile;
        }

        public static bool IsAdjacentOrOn(GameLocation location, Farmer player, Bush bush)
        {
            return CanInteractWith(location, player, bush);
        }

        public static Vector2 GetFaceTile(GameLocation location, Farmer player, Bush bush)
        {
            if (TryGetInteraction(location, player, bush, out Vector2 faceTile, out _))
                return faceTile;

            return GetClosestOccupiedTile(player.Tile, bush);
        }

        public static void SnapForBush(GameLocation location, Farmer player, Bush bush, Vector2 preferredStand)
        {
            if (!TrySnapForInteraction(location, player, bush, preferredStand))
                MovementHelper.TrySnapToStandTile(location, player, preferredStand);
        }

        public static bool TryGetAt(GameLocation location, Vector2 tile, out Bush bush)
        {
            foreach (var feature in location.largeTerrainFeatures)
            {
                if (feature is not Bush candidate)
                    continue;

                if (GetOccupiedTileSet(candidate).Contains(tile))
                {
                    bush = candidate;
                    return true;
                }
            }

            bush = null!;
            return false;
        }

        private static IEnumerable<BushInteraction> GetRankedInteractions(
            GameLocation location,
            Farmer player,
            Bush bush,
            Vector2 preferredStand)
        {
            HashSet<Vector2> reachable = WalkabilityHelper.GetReachableTiles(location, player.Tile);

            return GetInteractions(location, bush, reachable)
                .OrderBy(interaction => Vector2.DistanceSquared(interaction.StandTile, preferredStand))
                .ThenBy(interaction => Vector2.DistanceSquared(interaction.StandTile, player.Tile));
        }

        private static IEnumerable<BushInteraction> GetInteractions(
            GameLocation location,
            Bush bush,
            HashSet<Vector2> reachable)
        {
            var seen = new HashSet<Vector2>();
            HashSet<Vector2> occupied = GetOccupiedTileSet(bush);

            foreach (Vector2 bushTile in occupied)
            {
                foreach (Vector2 offset in CardinalOffsets)
                {
                    Vector2 stand = bushTile + offset;
                    if (occupied.Contains(stand) || !seen.Add(stand))
                        continue;

                    if (!reachable.Contains(stand))
                        continue;

                    if (!TryGetInteractionFromStand(location, stand, bush, out Vector2 faceTile, out int facing))
                        continue;

                    yield return new BushInteraction
                    {
                        StandTile = stand,
                        FaceTile = faceTile,
                        Facing = facing
                    };
                }
            }
        }

        private static bool TryGetInteractionFromStand(
            GameLocation location,
            Vector2 standTile,
            Bush bush,
            out Vector2 faceTile,
            out int facing)
        {
            faceTile = Vector2.Zero;
            facing = -1;
            HashSet<Vector2> occupied = GetOccupiedTileSet(bush);

            Vector2? bestFace = null;
            int bestFacing = -1;
            float bestDistance = float.MaxValue;

            foreach (Vector2 bushTile in occupied)
            {
                if (!IsCardinallyAdjacent(standTile, bushTile))
                    continue;

                int direction = GetFacingToward(standTile, bushTile);
                if (!IsInteractionClear(location, standTile, bushTile, direction, bush))
                    continue;

                float distance = Vector2.DistanceSquared(standTile, bushTile);
                if (distance >= bestDistance)
                    continue;

                bestDistance = distance;
                bestFace = bushTile;
                bestFacing = direction;
            }

            if (!bestFace.HasValue)
                return false;

            faceTile = bestFace.Value;
            facing = bestFacing;
            return true;
        }

        private static bool IsInteractionClear(
            GameLocation location,
            Vector2 standTile,
            Vector2 bushTile,
            int facing,
            Bush targetBush)
        {
            if (!IsFarmerStandTile(location, standTile, targetBush))
                return false;

            Vector2 frontTile = standTile + Utility.DirectionsTileVectors[facing];
            if (!GetOccupiedTileSet(targetBush).Contains(frontTile))
                return false;

            if (!GrabTileIntersectsBush(standTile, facing, targetBush))
                return false;

            if (!HasExclusiveGrabAccess(location, standTile, facing, targetBush))
                return false;

            if (IsTileBlockedForInteraction(location, bushTile, targetBush))
                return false;

            return true;
        }

        private static bool IsFarmerStandTile(GameLocation location, Vector2 standTile, Bush targetBush)
        {
            if (!WalkabilityHelper.CanFarmerStandOn(location, standTile))
                return false;

            LargeTerrainFeature standFeature = location.getLargeTerrainFeatureAt((int)standTile.X, (int)standTile.Y);
            if (standFeature != null && !ReferenceEquals(standFeature, targetBush))
                return false;

            if (location.objects.TryGetValue(standTile, out SObject standObject) && BlocksInteraction(standObject))
                return false;

            if (location.terrainFeatures.TryGetValue(standTile, out TerrainFeature standTerrain)
                && !standTerrain.isPassable(Game1.player))
                return false;

            return true;
        }

        private static bool HasExclusiveGrabAccess(GameLocation location, Vector2 standTile, int facing, Bush targetBush)
        {
            Rectangle grabRect = GetGrabRectangle(standTile, facing);
            bool hitsTarget = false;

            foreach (LargeTerrainFeature feature in location.largeTerrainFeatures)
            {
                if (!feature.getBoundingBox().Intersects(grabRect))
                    continue;

                if (ReferenceEquals(feature, targetBush))
                {
                    hitsTarget = true;
                    continue;
                }

                return false;
            }

            Vector2 frontTile = standTile + Utility.DirectionsTileVectors[facing];
            LargeTerrainFeature frontFeature = location.getLargeTerrainFeatureAt((int)frontTile.X, (int)frontTile.Y);
            if (frontFeature != null && !ReferenceEquals(frontFeature, targetBush))
                return false;

            return hitsTarget;
        }

        private static bool IsTileBlockedForInteraction(GameLocation location, Vector2 tile, Bush targetBush)
        {
            if (location.objects.TryGetValue(tile, out SObject obj) && BlocksInteraction(obj))
                return true;

            if (location.terrainFeatures.TryGetValue(tile, out TerrainFeature terrain) && !terrain.isPassable(Game1.player))
                return true;

            return false;
        }

        private static bool BlocksInteraction(SObject obj)
        {
            return !obj.isPassable();
        }

        private static bool IsCardinallyAdjacent(Vector2 standTile, Vector2 bushTile)
        {
            return (standTile.X == bushTile.X && Math.Abs(standTile.Y - bushTile.Y) == 1f)
                || (standTile.Y == bushTile.Y && Math.Abs(standTile.X - bushTile.X) == 1f);
        }

        private static int GetFacingToward(Vector2 standTile, Vector2 targetTile)
        {
            Vector2 diff = targetTile - standTile;

            if (Math.Abs(diff.X) > Math.Abs(diff.Y))
                return diff.X > 0 ? 1 : 3;

            if (diff.Y != 0)
                return diff.Y > 0 ? 2 : 0;

            return 2;
        }

        private static bool GrabTileIntersectsBush(Vector2 standTile, int facingDirection, Bush bush)
        {
            return bush.getBoundingBox().Intersects(GetGrabRectangle(standTile, facingDirection));
        }

        private static Rectangle GetGrabRectangle(Vector2 standTile, int facingDirection)
        {
            Vector2 grabTile = GetGrabTile(standTile, facingDirection);
            return new Rectangle((int)grabTile.X * Game1.tileSize, (int)grabTile.Y * Game1.tileSize, Game1.tileSize, Game1.tileSize);
        }

        private static Vector2 GetGrabTile(Vector2 standTile, int facingDirection)
        {
            var boundingBox = new Rectangle((int)standTile.X * Game1.tileSize, (int)standTile.Y * Game1.tileSize, Game1.tileSize, Game1.tileSize);

            return facingDirection switch
            {
                0 => new Vector2((boundingBox.X + boundingBox.Width / 2) / (float)Game1.tileSize, (boundingBox.Y - 5) / (float)Game1.tileSize),
                1 => new Vector2((boundingBox.X + boundingBox.Width + 5) / (float)Game1.tileSize, (boundingBox.Y + boundingBox.Height / 2) / (float)Game1.tileSize),
                2 => new Vector2((boundingBox.X + boundingBox.Width / 2) / (float)Game1.tileSize, (boundingBox.Y + boundingBox.Height + 5) / (float)Game1.tileSize),
                3 => new Vector2((boundingBox.X - 5) / (float)Game1.tileSize, (boundingBox.Y + boundingBox.Height / 2) / (float)Game1.tileSize),
                _ => standTile
            };
        }

        private static Vector2 GetClosestOccupiedTile(Vector2 fromTile, Bush bush)
        {
            Vector2 closest = bush.Tile;
            float bestDistance = float.MaxValue;

            foreach (Vector2 tile in GetOccupiedTiles(bush))
            {
                float distance = Vector2.DistanceSquared(fromTile, tile);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    closest = tile;
                }
            }

            return closest;
        }

        private static HashSet<Vector2> GetOccupiedTileSet(Bush bush)
        {
            var tiles = new HashSet<Vector2> { bush.Tile };

            Rectangle bounds = bush.getBoundingBox();
            int left = bounds.Left / Game1.tileSize;
            int top = bounds.Top / Game1.tileSize;
            int right = (bounds.Right - 1) / Game1.tileSize;
            int bottom = (bounds.Bottom - 1) / Game1.tileSize;

            for (int x = left; x <= right; x++)
            {
                for (int y = top; y <= bottom; y++)
                    tiles.Add(new Vector2(x, y));
            }

            return tiles;
        }
    }

}
