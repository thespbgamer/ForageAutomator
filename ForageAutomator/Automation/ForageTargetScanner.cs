using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using SObject = StardewValley.Object;

namespace ForageAutomator.Automation
{
    internal sealed class ForageTargetScanner
    {
        private const string ArtifactSpotId = "(O)590";
        private const string SeedSpotId = "(O)SeedSpot";
        private const string ForageItemTag = "forage_item";

        public IReadOnlyList<ForageTarget> Scan(GameLocation location, Farmer player, int? maxRadius = null)
        {
            var targets = new List<ForageTarget>();
            Vector2 playerTile = player.Tile;

            ScanGroundObjects(location, playerTile, maxRadius, targets);
            ScanForageCrops(location, playerTile, maxRadius, targets);
            ScanBushes(location, playerTile, maxRadius, targets);
            ScanPanningSpot(location, playerTile, maxRadius, targets);
            ScanCrabPots(location, playerTile, maxRadius, targets);
            ScanFruitTrees(location, playerTile, maxRadius, targets);
            ScanMachines(location, playerTile, maxRadius, targets);
            ScanTappers(location, playerTile, maxRadius, targets);
            ScanBeeHouses(location, playerTile, maxRadius, targets);
            ScanMushroomBoxes(location, playerTile, maxRadius, targets);
            ScanGarbageCans(location, playerTile, maxRadius, targets);
            ScanHayGrass(location, playerTile, maxRadius, targets);

            if (maxRadius.HasValue)
            {
                targets = targets
                    .Where(t => WalkabilityHelper.IsWithinRadius(playerTile, t.Tile, maxRadius.Value))
                    .ToList();
            }

            foreach (ForageTarget target in targets)
                target.DistanceFromPlayer = Vector2.Distance(playerTile, target.Tile);

            return targets.OrderBy(t => t.DistanceFromPlayer).ToList();
        }

        private static void ScanGroundObjects(GameLocation location, Vector2 playerTile, int? maxRadius, List<ForageTarget> targets)
        {
            foreach ((Vector2 tile, SObject obj) in location.objects.Pairs)
            {
                if (maxRadius.HasValue && !WalkabilityHelper.IsWithinRadius(playerTile, tile, maxRadius.Value))
                    continue;

                if (IsArtifactSpotObject(obj))
                {
                    targets.Add(new ForageTarget
                    {
                        Tile = tile,
                        Type = ForageType.ArtifactSpot,
                        RequiredTool = RequiredToolKind.Hoe,
                        Source = obj,
                        DisplayName = obj.DisplayName
                    });
                    continue;
                }

                if (!IsGroundForage(obj))
                    continue;

                targets.Add(new ForageTarget
                {
                    Tile = tile,
                    Type = ForageType.Ground,
                    RequiredTool = RequiredToolKind.None,
                    Source = obj,
                    DisplayName = obj.DisplayName
                });
            }
        }

        private static void ScanForageCrops(GameLocation location, Vector2 playerTile, int? maxRadius, List<ForageTarget> targets)
        {
            foreach ((Vector2 tile, TerrainFeature feature) in location.terrainFeatures.Pairs)
            {
                if (feature is not HoeDirt hoeDirt)
                    continue;

                if (!ForageCropHelper.IsHarvestable(hoeDirt))
                    continue;

                if (maxRadius.HasValue && !WalkabilityHelper.IsWithinRadius(playerTile, tile, maxRadius.Value))
                    continue;

                Crop crop = hoeDirt.crop;
                targets.Add(new ForageTarget
                {
                    Tile = tile,
                    Type = ForageType.ForageCrop,
                    RequiredTool = ForageCropHelper.GetRequiredTool(crop),
                    Source = hoeDirt,
                    DisplayName = ForageCropHelper.GetDisplayName(crop)
                });
            }
        }

        private static void ScanBushes(GameLocation location, Vector2 playerTile, int? maxRadius, List<ForageTarget> targets)
        {
            foreach (LargeTerrainFeature feature in location.largeTerrainFeatures)
            {
                if (feature is not Bush bush)
                    continue;

                if (bush.townBush.Value)
                    continue;

                if (!BushHelper.IsHarvestable(bush))
                    continue;

                Vector2 tile = bush.Tile;
                if (maxRadius.HasValue && !WalkabilityHelper.IsWithinRadius(playerTile, tile, maxRadius.Value))
                    continue;

                string? shakeItemId = bush.GetShakeOffItem();
                string displayName = shakeItemId != null
                    ? ItemRegistry.GetDataOrErrorItem(shakeItemId).DisplayName
                    : "bush";

                targets.Add(new ForageTarget
                {
                    Tile = tile,
                    Type = ForageType.Bush,
                    RequiredTool = RequiredToolKind.None,
                    Source = bush,
                    DisplayName = displayName
                });
            }
        }

        private static void ScanPanningSpot(GameLocation location, Vector2 playerTile, int? maxRadius, List<ForageTarget> targets)
        {
            Point panPoint = location.orePanPoint?.Value ?? Point.Zero;
            if (panPoint == Point.Zero)
                return;

            Vector2 tile = panPoint.ToVector2();
            if (maxRadius.HasValue && !WalkabilityHelper.IsWithinRadius(playerTile, tile, maxRadius.Value))
                return;

            targets.Add(new ForageTarget
            {
                Tile = tile,
                Type = ForageType.Panning,
                RequiredTool = RequiredToolKind.CopperPan,
                Source = panPoint,
                DisplayName = "panning spot"
            });
        }

        private static void ScanCrabPots(GameLocation location, Vector2 playerTile, int? maxRadius, List<ForageTarget> targets)
        {
            foreach ((Vector2 tile, SObject obj) in location.objects.Pairs)
            {
                if (!CrabPotHelper.IsHarvestable(obj))
                    continue;

                if (maxRadius.HasValue && !WalkabilityHelper.IsWithinRadius(playerTile, tile, maxRadius.Value))
                    continue;

                targets.Add(new ForageTarget
                {
                    Tile = tile,
                    Type = ForageType.CrabPot,
                    RequiredTool = RequiredToolKind.None,
                    Source = obj,
                    DisplayName = obj.DisplayName
                });
            }
        }

        private static void ScanFruitTrees(GameLocation location, Vector2 playerTile, int? maxRadius, List<ForageTarget> targets)
        {
            foreach ((Vector2 tile, TerrainFeature feature) in location.terrainFeatures.Pairs)
            {
                if (feature is not FruitTree tree || !FruitTreeHelper.IsHarvestable(tree))
                    continue;

                if (maxRadius.HasValue && !WalkabilityHelper.IsWithinRadius(playerTile, tile, maxRadius.Value))
                    continue;

                targets.Add(new ForageTarget
                {
                    Tile = tile,
                    Type = ForageType.FruitTree,
                    RequiredTool = RequiredToolKind.None,
                    Source = tree,
                    DisplayName = tree.GetDisplayName()
                });
            }
        }

        private static void ScanMachines(GameLocation location, Vector2 playerTile, int? maxRadius, List<ForageTarget> targets)
        {
            foreach ((Vector2 tile, SObject obj) in location.objects.Pairs)
            {
                if (!MachineHelper.IsHarvestable(obj))
                    continue;

                if (maxRadius.HasValue && !WalkabilityHelper.IsWithinRadius(playerTile, tile, maxRadius.Value))
                    continue;

                targets.Add(new ForageTarget
                {
                    Tile = tile,
                    Type = ForageType.Machine,
                    RequiredTool = RequiredToolKind.None,
                    Source = obj,
                    DisplayName = obj.DisplayName
                });
            }
        }

        private static void ScanTappers(GameLocation location, Vector2 playerTile, int? maxRadius, List<ForageTarget> targets)
        {
            foreach ((Vector2 tile, SObject obj) in location.objects.Pairs)
            {
                if (!TapperHelper.IsHarvestable(obj))
                    continue;

                if (maxRadius.HasValue && !WalkabilityHelper.IsWithinRadius(playerTile, tile, maxRadius.Value))
                    continue;

                targets.Add(new ForageTarget
                {
                    Tile = tile,
                    Type = ForageType.Tapper,
                    RequiredTool = RequiredToolKind.None,
                    Source = obj,
                    DisplayName = obj.DisplayName
                });
            }
        }

        private static void ScanBeeHouses(GameLocation location, Vector2 playerTile, int? maxRadius, List<ForageTarget> targets)
        {
            foreach ((Vector2 tile, SObject obj) in location.objects.Pairs)
            {
                if (!BeeHouseHelper.IsHarvestable(obj))
                    continue;

                if (maxRadius.HasValue && !WalkabilityHelper.IsWithinRadius(playerTile, tile, maxRadius.Value))
                    continue;

                targets.Add(new ForageTarget
                {
                    Tile = tile,
                    Type = ForageType.BeeHouse,
                    RequiredTool = RequiredToolKind.None,
                    Source = obj,
                    DisplayName = obj.DisplayName
                });
            }
        }

        private static void ScanMushroomBoxes(GameLocation location, Vector2 playerTile, int? maxRadius, List<ForageTarget> targets)
        {
            foreach ((Vector2 tile, SObject obj) in location.objects.Pairs)
            {
                if (!MushroomBoxHelper.IsHarvestable(obj))
                    continue;

                if (maxRadius.HasValue && !WalkabilityHelper.IsWithinRadius(playerTile, tile, maxRadius.Value))
                    continue;

                targets.Add(new ForageTarget
                {
                    Tile = tile,
                    Type = ForageType.MushroomBox,
                    RequiredTool = RequiredToolKind.None,
                    Source = obj,
                    DisplayName = obj.DisplayName
                });
            }
        }

        private static void ScanGarbageCans(GameLocation location, Vector2 playerTile, int? maxRadius, List<ForageTarget> targets)
        {
            foreach (GarbageCanInfo can in GarbageCanHelper.GetGarbageCans(location))
            {
                if (maxRadius.HasValue && !WalkabilityHelper.IsWithinRadius(playerTile, can.Tile, maxRadius.Value))
                    continue;

                targets.Add(new ForageTarget
                {
                    Tile = can.Tile,
                    Type = ForageType.GarbageCan,
                    RequiredTool = RequiredToolKind.None,
                    Source = can,
                    DisplayName = "garbage can"
                });
            }
        }

        private static void ScanHayGrass(GameLocation location, Vector2 playerTile, int? maxRadius, List<ForageTarget> targets)
        {
            foreach ((Vector2 tile, TerrainFeature feature) in location.terrainFeatures.Pairs)
            {
                if (feature is not Grass grass || !HayGrassHelper.IsHarvestable(location, tile, grass))
                    continue;

                if (maxRadius.HasValue && !WalkabilityHelper.IsWithinRadius(playerTile, tile, maxRadius.Value))
                    continue;

                targets.Add(new ForageTarget
                {
                    Tile = tile,
                    Type = ForageType.HayGrass,
                    RequiredTool = RequiredToolKind.Scythe,
                    Source = grass,
                    DisplayName = "grass"
                });
            }
        }

        private static bool IsGroundForage(SObject obj)
        {
            if (obj.IsSpawnedObject)
                return true;

            return obj.GetContextTags().Contains(ForageItemTag);
        }

        internal static bool IsArtifactSpotObject(SObject obj)
        {
            if (obj.QualifiedItemId == ArtifactSpotId || obj.QualifiedItemId == SeedSpotId)
                return true;

            return obj.ItemId is "590" or "SeedSpot";
        }

        private static bool IsArtifactSpot(SObject obj) => IsArtifactSpotObject(obj);

        public void ApplyReachability(
            GameLocation location,
            Farmer player,
            IEnumerable<ForageTarget> targets,
            bool usePathfinding,
            bool blockGarbageWhenWitnessed)
        {
            HashSet<Vector2>? reachable = usePathfinding
                ? WalkabilityHelper.GetReachableTiles(location, player.Tile)
                : null;

            foreach (ForageTarget target in targets)
            {
                if (target.Type == ForageType.Bush && target.Source is Bush bush
                    && BushHelper.CanInteractWith(location, player, bush))
                {
                    target.StandTile = player.Tile;
                    continue;
                }

                if (target.Type == ForageType.Panning)
                {
                    Pan? pan = ToolHelper.FindCopperPan(player);

                    if (PanningHelper.CanInteractFrom(location, player, target.Tile, pan))
                    {
                        target.StandTile = player.Tile;
                        continue;
                    }

                    Vector2? panStand = usePathfinding && reachable != null
                        ? PanningHelper.FindStandTile(location, player, target.Tile, reachable, pan)
                        : PanningHelper.FindStandTileAnywhere(location, player, target.Tile, pan);
                    if (!panStand.HasValue)
                    {
                        target.SkipReason = SkipReason.Unreachable;
                        continue;
                    }

                    target.StandTile = panStand.Value;
                    continue;
                }

                if (target.Type == ForageType.GarbageCan)
                {
                    if (GarbageCanHelper.ShouldBlockWitness(blockGarbageWhenWitnessed, location, player))
                        target.SkipReason = SkipReason.NpcWitness;

                    if (WalkabilityHelper.IsAdjacentOrOn(player.Tile, target.Tile))
                    {
                        target.StandTile = player.Tile;
                        continue;
                    }

                    Vector2? garbageStand = usePathfinding && reachable != null
                        ? WalkabilityHelper.FindStandTile(location, target.Tile, reachable)
                        : WalkabilityHelper.FindNearbyStandTile(location, target.Tile);
                    if (!garbageStand.HasValue && target.SkipReason != SkipReason.NpcWitness)
                        target.SkipReason = SkipReason.Unreachable;

                    if (garbageStand.HasValue)
                        target.StandTile = garbageStand.Value;

                    continue;
                }

                if (target.Type.IsOtherInteraction())
                {
                    if (WalkabilityHelper.IsAdjacentOrOn(player.Tile, target.Tile))
                    {
                        target.StandTile = player.Tile;
                        continue;
                    }

                    Vector2? interactionStand = usePathfinding && reachable != null
                        ? WalkabilityHelper.FindStandTile(location, target.Tile, reachable)
                        : WalkabilityHelper.FindNearbyStandTile(location, target.Tile);
                    if (!interactionStand.HasValue)
                    {
                        target.SkipReason = SkipReason.Unreachable;
                        continue;
                    }

                    target.StandTile = interactionStand.Value;
                    continue;
                }

                if (WalkabilityHelper.IsAdjacentOrOn(player.Tile, target.Tile))
                {
                    target.StandTile = player.Tile;
                    continue;
                }

                Vector2? stand = target.Type == ForageType.Bush && target.Source is Bush bushTarget
                    ? BushHelper.FindStandTile(location, player, bushTarget, reachable, usePathfinding)
                    : usePathfinding && reachable != null
                        ? WalkabilityHelper.FindStandTile(location, target.Tile, reachable)
                        : WalkabilityHelper.FindNearbyStandTile(location, target.Tile);

                if (!stand.HasValue)
                {
                    target.SkipReason = SkipReason.Unreachable;
                    continue;
                }

                target.StandTile = stand.Value;
            }
        }
    }

}
