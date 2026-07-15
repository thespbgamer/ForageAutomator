using System.Collections.Generic;
using System.Linq;
using ForageAutomator.Automation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace ForageAutomator.Rendering
{
    internal sealed class ForageLineRenderer
    {
        private const int LineUpdateInterval = 60;

        private readonly ModConfig config;
        private readonly PassivePickupController passiveController;
        private readonly ForageRunController runController;
        private readonly ForageScanCache scanCache;
        private readonly List<ForageTarget> visibleTargets = new();
        private int tickCounter;
        private bool refreshRequested;
        private bool hiddenForSweep;

        public ForageLineRenderer(
            ModConfig config,
            PassivePickupController passiveController,
            ForageRunController runController,
            ForageScanCache scanCache)
        {
            this.config = config;
            this.passiveController = passiveController;
            this.runController = runController;
            this.scanCache = scanCache;
        }

        public void Invalidate()
        {
            refreshRequested = true;
            visibleTargets.Clear();
        }

        public void UpdateTicked()
        {
            if (!Context.IsWorldReady)
                return;

            if (runController.IsRunning)
            {
                hiddenForSweep = true;
                visibleTargets.Clear();
                return;
            }

            if (hiddenForSweep)
            {
                hiddenForSweep = false;
                refreshRequested = true;
            }

            if (!config.ShowTargetLines)
            {
                visibleTargets.Clear();
                return;
            }

            tickCounter++;
            GameLocation location = Game1.currentLocation;
            Farmer player = Game1.player;
            bool cacheRefreshed = scanCache.TryRefreshIfNeeded(location, player);
            if (!refreshRequested && !cacheRefreshed && tickCounter % LineUpdateInterval != 0)
                return;

            refreshRequested = false;
            RefreshVisibleTargets(location, player);
        }

        public void OnRenderedWorld(object? sender, RenderedWorldEventArgs e)
        {
            if (!ShouldDraw())
                return;

            DrawTargets(e.SpriteBatch);
        }

        private bool ShouldDraw()
        {
            return config.ShowTargetLines
                && Context.IsWorldReady
                && !runController.IsRunning
                && visibleTargets.Count > 0
                && Game1.currentLocation != null
                && !Game1.eventUp
                && Game1.gameMode == 3;
        }

        private void DrawTargets(SpriteBatch spriteBatch)
        {
            OverlayDrawHelper.EnsurePixelTexture();

            Farmer player = Game1.player;
            Vector2 playerWorld = OverlayDrawHelper.GetPlayerWorldAnchor(player);

            foreach (ForageTarget target in visibleTargets)
            {
                if (!IsTargetStillVisible(target))
                    continue;

                if (!ForageTargetFilters.IsEnabledForLines(config, target.Type))
                    continue;

                Vector2 targetWorld = GetTargetWorldCenter(target);
                Color color = GetLineColor(target);

                OverlayDrawHelper.DrawLine(spriteBatch, playerWorld, targetWorld, color);
                OverlayDrawHelper.DrawMarker(spriteBatch, targetWorld, color);
            }
        }

        private void RefreshVisibleTargets(GameLocation location, Farmer player)
        {
            visibleTargets.Clear();

            int? maxRadius = ForageTargetFilters.GetLineScanRadius(config);
            IReadOnlyList<ForageTarget> scanned = maxRadius.HasValue
                ? scanCache.GetTargetsInTileRadius(location, player, maxRadius.Value)
                : scanCache.GetAllTargets(location, player);

            foreach (ForageTarget target in ForageTargetFilters.FilterForLines(config, scanned))
            {
                if (target.Type == ForageType.Panning && !PanningHelper.IsActivePanTile(location, target.Tile))
                    continue;

                ClassifyTarget(player, target);

                if (target.SkipReason == SkipReason.EmptyBush && !config.ShowLinesEmptyBushes)
                    continue;

                visibleTargets.Add(target);
            }

            if (config.ShowLinesEmptyBushes && config.ShowLinesBushes)
                AddEmptyBushTargets(location, player);

            foreach (ForageTarget skipped in passiveController.SkippedTargets)
            {
                if (!ShouldIncludeSkippedTarget(location, player, skipped))
                    continue;

                if (!visibleTargets.Any(t => t.Tile == skipped.Tile))
                    visibleTargets.Add(skipped);
            }

            foreach (ForageTarget skipped in runController.SkippedTargets)
            {
                if (!ShouldIncludeSkippedTarget(location, player, skipped))
                    continue;

                if (!visibleTargets.Any(t => t.Tile == skipped.Tile))
                    visibleTargets.Add(skipped);
            }
        }

        private void AddEmptyBushTargets(GameLocation location, Farmer player)
        {
            int? maxRadius = ForageTargetFilters.GetLineScanRadius(config);

            foreach (var feature in location.largeTerrainFeatures)
            {
                if (feature is not Bush bush)
                    continue;

                if (!BushHelper.IsInSeasonWithoutProduce(bush))
                    continue;

                Vector2 tile = bush.Tile;
                if (maxRadius.HasValue && !WalkabilityHelper.IsWithinRadius(player.Tile, tile, maxRadius.Value))
                    continue;

                if (visibleTargets.Any(t => t.Tile == tile))
                    continue;

                visibleTargets.Add(new ForageTarget
                {
                    Tile = tile,
                    Type = ForageType.Bush,
                    RequiredTool = RequiredToolKind.None,
                    Source = bush,
                    DisplayName = "bush",
                    SkipReason = SkipReason.EmptyBush
                });
            }
        }

        private bool ShouldIncludeSkippedTarget(GameLocation location, Farmer player, ForageTarget target)
        {
            if (!ForageTargetFilters.IsEnabledForLines(config, target.Type))
                return false;

            if (target.Type == ForageType.Panning && !PanningHelper.IsActivePanTile(location, target.Tile))
                return false;

            if (!IsTargetStillVisible(target))
                return false;

            ClassifyTarget(player, target);
            return true;
        }

        private void ClassifyTarget(Farmer player, ForageTarget target)
        {
            if (target.Type == ForageType.Bush && target.Source is Bush bush && !BushHelper.IsHarvestable(bush))
            {
                target.SkipReason = SkipReason.EmptyBush;
                return;
            }

            if (!ToolHelper.HasRequiredTool(player, target.RequiredTool))
            {
                target.SkipReason = SkipReason.MissingTool;
                return;
            }

            if (!ToolHelper.HasInventorySpace(player))
            {
                target.SkipReason = SkipReason.InventoryFull;
                return;
            }

            if (target.Type == ForageType.GarbageCan
                && config.OtherInteractions.GarbageCans.BlockWhenWitnessed
                && GarbageCanHelper.HasWitnessingNpc(Game1.currentLocation, player))
            {
                target.SkipReason = SkipReason.NpcWitness;
                return;
            }

            if (target.SkipReason == SkipReason.Unreachable)
            {
                if (target.Type == ForageType.Panning
                    && PanningHelper.FindStandTileAnywhere(Game1.currentLocation, player, target.Tile) != null)
                {
                    target.SkipReason = SkipReason.OutOfRange;
                }
                else
                {
                    return;
                }
            }

            if (!CollectionHelper.IsWithinPickupRange(player, target.Tile, config.PickupRadius))
            {
                target.SkipReason = SkipReason.OutOfRange;
                return;
            }

            if (target.Type == ForageType.Panning)
            {
                target.SkipReason = PanningHelper.IsReadyToCollect(Game1.currentLocation, player, target)
                    ? SkipReason.None
                    : SkipReason.OutOfRange;
                return;
            }

            target.SkipReason = SkipReason.None;
        }

        private bool IsTargetStillVisible(ForageTarget target)
        {
            if (target.Type != ForageType.Bush)
                return IsTargetStillOnMap(target);

            if (!BushHelper.TryGetAt(Game1.currentLocation, target.Tile, out Bush bush))
                return false;

            if (BushHelper.IsHarvestable(bush))
                return true;

            return config.ShowLinesEmptyBushes && BushHelper.IsInSeasonWithoutProduce(bush);
        }

        private static Vector2 GetTargetWorldCenter(ForageTarget target)
        {
            if (target.Type == ForageType.Bush && target.Source is Bush bush)
            {
                Rectangle bounds = bush.getBoundingBox();
                return new Vector2(bounds.Center.X, bounds.Center.Y);
            }

            return CollectionHelper.GetTileCenter(target.Tile);
        }

        private static bool IsTargetStillOnMap(ForageTarget target)
        {
            GameLocation location = Game1.currentLocation;

            return target.Type switch
            {
                ForageType.Ground or ForageType.ArtifactSpot => location.objects.ContainsKey(target.Tile),
                ForageType.ForageCrop => ForageCropHelper.TryGetAt(location, target.Tile, out _),
                ForageType.Panning => PanningHelper.IsActivePanTile(location, target.Tile),
                ForageType.CrabPot => CrabPotHelper.TryGetAt(location, target.Tile, out _),
                ForageType.FruitTree => FruitTreeHelper.TryGetAt(location, target.Tile, out FruitTree? tree) && FruitTreeHelper.IsHarvestable(tree),
                ForageType.Machine => MachineHelper.TryGetAt(location, target.Tile, out _),
                ForageType.Tapper => TapperHelper.TryGetAt(location, target.Tile, out _),
                ForageType.BeeHouse => BeeHouseHelper.TryGetAt(location, target.Tile, out _),
                ForageType.MushroomBox => MushroomBoxHelper.TryGetAt(location, target.Tile, out _),
                ForageType.GarbageCan => target.Source is GarbageCanInfo can && GarbageCanHelper.CanCheckToday(can.Id),
                ForageType.HayGrass => HayGrassHelper.TryGetAt(location, target.Tile, out Grass? grass) && HayGrassHelper.IsHarvestable(location, target.Tile, grass),
                _ => false
            };
        }

        private Color GetLineColor(ForageTarget target)
        {
            return target.SkipReason switch
            {
                SkipReason.InventoryFull => ConfigColor.Parse(config.ColorLineInventoryFull, ConfigColor.InventoryFull),
                SkipReason.MissingTool => ConfigColor.Parse(config.ColorLineMissingTool, ConfigColor.MissingTool),
                SkipReason.Unreachable => ConfigColor.Parse(config.ColorLineUnreachable, ConfigColor.Unreachable),
                SkipReason.OutOfRange => ConfigColor.Parse(config.ColorLineOutOfRange, ConfigColor.OutOfRange),
                SkipReason.OnHorse => ConfigColor.Parse(config.ColorLineOutOfRange, ConfigColor.OutOfRange),
                SkipReason.EmptyBush => ConfigColor.Parse(config.ColorLineEmptyBush, ConfigColor.EmptyBush),
                SkipReason.NpcWitness => ConfigColor.NpcWitness,
                SkipReason.None when target.Type.IsOtherInteraction() =>
                    ConfigColor.Parse(config.OtherInteractions.GetRule(target.Type).LineColor, ConfigColor.Ready),
                _ => ConfigColor.Parse(config.ColorLineReady, ConfigColor.Ready)
            };
        }
    }

}
