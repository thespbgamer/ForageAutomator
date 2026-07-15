using System.Collections.Generic;
using System.Linq;
using ForageAutomator.Notifications;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace ForageAutomator.Automation
{
    internal sealed class PassivePickupController
    {
        private const int ActionHoldTicks = 15;

        private readonly ModConfig config;
        private readonly HudNotifier notifier;
        private readonly ForageScanCache scanCache;
        private readonly ForageCollectionService collectionService;
        private readonly List<ForageTarget> skippedTargets = new();
        private int tickCounter;
        private int actionHoldTicks;
        private Vector2 holdTile;
        private Vector2 lastPlayerPosition;
        private bool hasLastPlayerPosition;

        public IReadOnlyList<ForageTarget> SkippedTargets => skippedTargets;

        public void ClearSkippedTargets()
        {
            skippedTargets.Clear();
        }

        public PassivePickupController(ModConfig config, HudNotifier notifier, ForageScanCache scanCache)
        {
            this.config = config;
            this.notifier = notifier;
            this.scanCache = scanCache;
            collectionService = new ForageCollectionService(config, notifier);
        }

        public void UpdateTicked(bool mapCollectRunning)
        {
            if (!config.AutoCollectOnRange || mapCollectRunning)
                return;

            if (!Context.IsWorldReady)
                return;

            Farmer player = Game1.player;
            GameLocation location = Game1.currentLocation;

            if (actionHoldTicks > 0)
            {
                if (MovementHelper.IsRidingHorse(player))
                {
                    actionHoldTicks = 0;
                    return;
                }

                MaintainActionHold(player, location);
                return;
            }

            bool highSpeed = MovementHelper.IsHighSpeedMovement(player, ref lastPlayerPosition, hasLastPlayerPosition);
            hasLastPlayerPosition = true;
            lastPlayerPosition = player.Position;

            tickCounter++;
            int interval = (player.isMoving() || highSpeed) ? 5 : 15;
            if (tickCounter % interval != 0)
                return;

            if (!AutomationGate.CanAutomate(player))
                return;

            if (!CollectPolicy.IsLocationAllowed(config, location, CollectScope.Auto))
                return;

            if (player.UsingTool || player.FarmerSprite.PauseForSingleAnimation)
            {
                if (!TryClearStuckPanAnimation(location, player))
                    return;
            }

            bool ridingHorse = MovementHelper.IsRidingHorse(player);

            int scanRadius = config.PickupRadius + (highSpeed ? 1 : 0);
            IReadOnlyList<ForageTarget> targets = ForageTargetFilters
                .FilterForCollect(config, scanCache.GetTargetsInRadius(location, player, scanRadius), CollectScope.Auto, location)
                .ToList();

            skippedTargets.Clear();
            bool hasInRangeTarget = false;

            foreach (ForageTarget target in targets)
            {
                int pickupRadius = config.PickupRadius + (highSpeed ? 1 : 0);

                if (target.Type == ForageType.Panning)
                {
                    if (!PanningHelper.IsWithinPassivePickupRange(location, player, target, pickupRadius))
                    {
                        if (config.ShowTargetLines)
                        {
                            target.SkipReason = SkipReason.OutOfRange;
                            skippedTargets.Add(target);
                        }
                        continue;
                    }
                }
                else if (!CollectionHelper.IsWithinPickupRange(player, target.Tile, pickupRadius))
                {
                    if (config.ShowTargetLines)
                    {
                        target.SkipReason = SkipReason.OutOfRange;
                        skippedTargets.Add(target);
                    }
                    continue;
                }

                if (ridingHorse)
                {
                    hasInRangeTarget = true;
                    if (config.ShowTargetLines)
                    {
                        target.SkipReason = SkipReason.OnHorse;
                        skippedTargets.Add(target);
                    }
                    continue;
                }

                if (target.SkipReason == SkipReason.Unreachable)
                {
                    if (config.ShowTargetLines)
                        skippedTargets.Add(target);
                    continue;
                }

                if (!ToolHelper.HasRequiredTool(player, target.RequiredTool))
                {
                    target.SkipReason = SkipReason.MissingTool;
                    if (config.ShowTargetLines)
                        skippedTargets.Add(target);
                    notifier.ShowMissingTool(target.RequiredTool);
                    continue;
                }

                if (!ToolHelper.HasInventorySpace(player))
                {
                    target.SkipReason = SkipReason.InventoryFull;
                    if (config.ShowTargetLines)
                        skippedTargets.Add(target);
                    continue;
                }

                if (target.Type == ForageType.GarbageCan
                    && GarbageCanHelper.ShouldBlockWitness(config.OtherInteractions.GarbageCans.BlockWhenWitnessed, location, player))
                {
                    target.SkipReason = SkipReason.NpcWitness;
                    if (config.OtherInteractions.GarbageCans.ShowLines)
                        skippedTargets.Add(target);
                    continue;
                }

                if (target.Type == ForageType.Panning)
                {
                    if (!PanningHelper.IsReadyToCollect(location, player, target))
                    {
                        if (!PanningHelper.TryMoveToPanStand(location, player, target))
                            continue;
                    }

                    CollectResult panResult = collectionService.TryCollect(location, player, target);
                    PanningHelper.ClearPanAnimationState(player);

                    if (panResult != CollectResult.Success && config.ShowTargetLines)
                        skippedTargets.Add(target);

                    continue;
                }

                if (!MovementHelper.IsCloseEnoughToCollect(player, target))
                {
                    if (highSpeed && CollectionHelper.IsWithinPickupRange(player, target.Tile, pickupRadius + 1))
                        SnapToTarget(player, target);
                    else
                        continue;
                }

                CollectResult result = collectionService.TryCollect(location, player, target);

                if (result != CollectResult.Success && highSpeed)
                {
                    SnapToTarget(player, target);
                    result = collectionService.TryCollect(location, player, target);
                }

                MovementHelper.ReleasePlayerControlIfNeeded(player);

                if (result == CollectResult.Success && ForageItemHelper.DropsOnGround(target))
                    BeginActionHold(player, location, target.Tile);
                else if (result != CollectResult.Success && config.ShowTargetLines)
                    skippedTargets.Add(target);
            }

            if (ridingHorse && hasInRangeTarget)
                notifier.ShowRidingHorseBlocked();
        }

        private static bool TryClearStuckPanAnimation(GameLocation location, Farmer player)
        {
            if (!PanningHelper.TryGetPanTile(location, out Vector2 panTile))
                return false;

            if (!PanningHelper.CanInteractFrom(location, player, panTile, ToolHelper.FindCopperPan(player)))
                return false;

            PanningHelper.ClearPanAnimationState(player);
            return true;
        }

        private void BeginActionHold(Farmer player, GameLocation location, Vector2 tile)
        {
            holdTile = tile;
            actionHoldTicks = ActionHoldTicks;
            MaintainActionHold(player, location);
        }

        private void MaintainActionHold(Farmer player, GameLocation location)
        {
            player.controller = null;
            DebrisPickupHelper.CollectNearTile(location, player, holdTile);
            actionHoldTicks--;

            if (actionHoldTicks <= 0)
                MovementHelper.ReleasePlayerControlIfNeeded(player);
        }

        private void SnapToTarget(Farmer player, ForageTarget target)
        {
            GameLocation location = Game1.currentLocation;
            bool usePathfinding = MovementHelper.ShouldUsePathfinding(config, player, ref lastPlayerPosition, hasLastPlayerPosition);

            if (target.SkipReason == SkipReason.Unreachable)
                return;

            if (target.Type == ForageType.Panning)
            {
                PanningHelper.TryMoveToPanStand(location, player, target);
                return;
            }

            Vector2 stand = MovementHelper.GetStandOrTargetTile(target);
            if (usePathfinding && !WalkabilityHelper.IsReachable(location, player.Tile, stand))
                return;

            if (target.Type == ForageType.Bush && target.Source is Bush bush)
                BushHelper.SnapForBush(location, player, bush, stand, usePathfinding);
            else if (MovementHelper.TrySnapToStandTile(location, player, stand, usePathfinding))
                CollectionHelper.PreparePlayer(player, target.Tile);
        }
    }

}
