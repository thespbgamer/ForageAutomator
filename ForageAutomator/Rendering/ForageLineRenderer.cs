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
        private const int LineUpdateInterval = 5;

        private readonly ModConfig config;
        private readonly PassivePickupController passiveController;
        private readonly ForageRunController runController;
        private readonly ForageTargetScanner scanner = new();
        private readonly List<ForageTarget> visibleTargets = new();
        private int tickCounter;
        private bool refreshRequested;
        private bool hiddenForSweep;

        public ForageLineRenderer(ModConfig config, PassivePickupController passiveController, ForageRunController runController)
        {
            this.config = config;
            this.passiveController = passiveController;
            this.runController = runController;
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
            if (!refreshRequested && tickCounter % LineUpdateInterval != 0)
                return;

            refreshRequested = false;
            RefreshVisibleTargets();
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
                Color color = GetLineColor(target.SkipReason);

                OverlayDrawHelper.DrawLine(spriteBatch, playerWorld, targetWorld, color);
                OverlayDrawHelper.DrawMarker(spriteBatch, targetWorld, color);
            }
        }

        private void RefreshVisibleTargets()
        {
            visibleTargets.Clear();

            GameLocation location = Game1.currentLocation;
            Farmer player = Game1.player;

            IReadOnlyList<ForageTarget> scanned = scanner.Scan(
                location,
                player,
                ForageTargetFilters.GetLineScanRadius(config));
            scanner.ApplyReachability(location, player, scanned);

            foreach (ForageTarget target in ForageTargetFilters.FilterForLines(config, scanned))
            {
                ClassifyTarget(player, target);

                if (target.SkipReason == SkipReason.EmptyBush && !config.ShowLinesEmptyBushes)
                    continue;

                visibleTargets.Add(target);
            }

            if (config.ShowLinesEmptyBushes && config.ShowLinesBushes)
                AddEmptyBushTargets(location, player);

            foreach (ForageTarget skipped in passiveController.SkippedTargets)
            {
                if (!ShouldIncludeSkippedTarget(skipped))
                    continue;

                if (!visibleTargets.Any(t => t.Tile == skipped.Tile))
                    visibleTargets.Add(skipped);
            }

            foreach (ForageTarget skipped in runController.SkippedTargets)
            {
                if (!ShouldIncludeSkippedTarget(skipped))
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

        private bool ShouldIncludeSkippedTarget(ForageTarget target)
        {
            if (!ForageTargetFilters.IsEnabledForLines(config, target.Type))
                return false;

            return IsTargetStillVisible(target);
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

            if (target.SkipReason == SkipReason.Unreachable)
                return;

            if (!CollectionHelper.IsWithinPickupRange(player, target.Tile, config.PickupRadius))
                target.SkipReason = SkipReason.OutOfRange;
            else
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
                ForageType.Panning => (location.orePanPoint?.Value ?? Point.Zero) != Point.Zero,
                _ => false
            };
        }

        private Color GetLineColor(SkipReason reason)
        {
            return reason switch
            {
                SkipReason.InventoryFull => ConfigColor.Parse(config.ColorLineInventoryFull, ConfigColor.InventoryFull),
                SkipReason.MissingTool => ConfigColor.Parse(config.ColorLineMissingTool, ConfigColor.MissingTool),
                SkipReason.Unreachable => ConfigColor.Parse(config.ColorLineUnreachable, ConfigColor.Unreachable),
                SkipReason.OutOfRange => ConfigColor.Parse(config.ColorLineOutOfRange, ConfigColor.OutOfRange),
                SkipReason.OnHorse => ConfigColor.Parse(config.ColorLineOutOfRange, ConfigColor.OutOfRange),
                SkipReason.EmptyBush => ConfigColor.Parse(config.ColorLineEmptyBush, ConfigColor.EmptyBush),
                _ => ConfigColor.Parse(config.ColorLineReady, ConfigColor.Ready)
            };
        }
    }

}
