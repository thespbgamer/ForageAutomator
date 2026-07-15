using System.Collections.Generic;
using ForageAutomator.Notifications;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Pathfinding;
using StardewValley.TerrainFeatures;

namespace ForageAutomator.Automation
{
    internal enum SweepState
    {
        Idle,
        Scanning,
        Moving,
        Settling,
        Collecting,
        AwaitingDebris,
        Done
    }

    internal sealed class ForageRunController
    {
        private readonly ModConfig config;
        private readonly ForageScanCache scanCache;
        private readonly ForageCollectionService collectionService;
        private readonly HudNotifier notifier;
        private readonly List<ForageTarget> queue = new();
        private readonly List<ForageTarget> skippedTargets = new();
        private ForageTarget? currentTarget;
        private SweepState state = SweepState.Idle;
        private int collectedCount;
        private int experienceAtSweepStart;
        private int settleCounter;
        private int settleAttempts;
        private int debrisPickupTicks;
        private bool usedSnapForCurrent;
        private Vector2 returnTile;
        private string? sweepLocationName;
        private bool pendingAutoStart;
        private int pendingAutoStartWarmupTicks;
        private bool autoSweepSuppressedUntilWarp;
        private const int AutoStartWarmupTicks = 15;
        private int? sweepRadiusTiles;
        private Vector2 lastPlayerPosition;
        private bool hasLastPlayerPosition;
        private CollectScope currentSweepScope = CollectScope.Manual;

        public bool IsRunning => state != SweepState.Idle && state != SweepState.Done;
        public SweepState State => state;
        public IReadOnlyList<ForageTarget> SkippedTargets => skippedTargets;
        public int QueueCount => queue.Count;

        public void ClearSkippedTargets()
        {
            skippedTargets.Clear();
        }

        public ForageRunController(ModConfig config, HudNotifier notifier, ForageScanCache scanCache)
        {
            this.config = config;
            this.scanCache = scanCache;
            this.notifier = notifier;
            collectionService = new ForageCollectionService(config, notifier);
        }

        public void StartWholeMap()
        {
            autoSweepSuppressedUntilWarp = false;
            StartSweep(maxRadius: null);
        }

        public void StartRange()
        {
            autoSweepSuppressedUntilWarp = false;
            StartSweep(maxRadius: config.PickupRadius);
        }

        private void StartSweep(int? maxRadius)
        {
            if (!Context.IsWorldReady)
                return;

            if (IsRunning)
            {
                Cancel();
                return;
            }

            if (MovementHelper.IsRidingHorse(Game1.player))
            {
                notifier.ShowRidingHorseBlocked();
                return;
            }

            if (!AutomationGate.CanAutomate(Game1.player))
                return;

            if (!CollectPolicy.IsLocationAllowed(config, Game1.currentLocation, CollectScope.Manual))
            {
                notifier.ShowSweepBlocked(CollectScope.Manual);
                return;
            }

            BeginSweep(maxRadius, CollectScope.Manual);
        }

        private void StartAutomatic()
        {
            if (!config.AutoCollectWholeMap || !Context.IsWorldReady || IsRunning)
                return;

            if (MovementHelper.IsRidingHorse(Game1.player))
            {
                notifier.ShowRidingHorseBlocked();
                return;
            }

            if (!AutomationGate.CanAutomate(Game1.player))
                return;

            if (!CollectPolicy.IsLocationAllowed(config, Game1.currentLocation, CollectScope.Auto))
            {
                if (HasCollectableTargets(CollectScope.Auto, requireLocationAllowed: false))
                    notifier.ShowSweepBlocked(CollectScope.Auto);

                return;
            }

            BeginSweep(maxRadius: null, CollectScope.Auto);
        }

        private void BeginSweep(int? maxRadius, CollectScope scope)
        {
            scanCache.Invalidate();
            sweepRadiusTiles = maxRadius;
            currentSweepScope = scope;
            returnTile = Game1.player.Tile;
            sweepLocationName = Game1.currentLocation.NameOrUniqueName;
            experienceAtSweepStart = ExperienceTracker.CaptureTotal(Game1.player);
            ResetRunState();
            state = SweepState.Scanning;

            if (!BuildQueue())
            {
                sweepLocationName = null;
                state = SweepState.Idle;
                return;
            }

            notifier.ShowSweepStarted();
            AdvanceToNextTarget();
        }

        public void ScheduleAutoStart()
        {
            if (!config.AutoCollectWholeMap || autoSweepSuppressedUntilWarp)
                return;

            pendingAutoStart = true;
            pendingAutoStartWarmupTicks = AutoStartWarmupTicks;
        }

        public void Cancel()
        {
            Cancel(returnToStart: config.ReturnToStartAfterSweep, suppressAutoUntilWarp: true);
        }

        public void Cancel(bool returnToStart)
        {
            Cancel(returnToStart, suppressAutoUntilWarp: true);
        }

        public void Cancel(bool returnToStart, bool suppressAutoUntilWarp)
        {
            pendingAutoStart = false;
            pendingAutoStartWarmupTicks = 0;

            if (suppressAutoUntilWarp)
                autoSweepSuppressedUntilWarp = true;

            if (!IsRunning)
                return;

            Game1.player.controller = null;
            MovementHelper.ReleasePlayerControl(Game1.player);
            queue.Clear();
            currentTarget = null;
            settleCounter = 0;
            settleAttempts = 0;
            state = SweepState.Idle;

            if (returnToStart)
                ReturnToStartPosition();

            sweepLocationName = null;
            notifier.ShowSweepCancelled();
        }

        public void OnWarped()
        {
            autoSweepSuppressedUntilWarp = false;
            WalkabilityHelper.InvalidateReachabilityCache();

            if (IsRunning)
                Cancel(returnToStart: false, suppressAutoUntilWarp: false);

            OnLocationContentChanged();
        }

        public void OnCollectSettingsChanged()
        {
            WalkabilityHelper.InvalidateReachabilityCache();
            skippedTargets.Clear();

            if (IsRunning)
                Cancel(returnToStart: false, suppressAutoUntilWarp: false);

            if (config.AutoCollectWholeMap)
                ScheduleAutoStart();
        }

        public void OnLocationContentChanged()
        {
            WalkabilityHelper.InvalidateReachabilityCache();

            if (!config.AutoCollectWholeMap || IsRunning)
                return;

            if (HasCollectableTargets(CollectScope.Auto))
            {
                ScheduleAutoStart();
                return;
            }

            if (!CollectPolicy.IsLocationAllowed(config, Game1.currentLocation, CollectScope.Auto)
                && HasCollectableTargets(CollectScope.Auto, requireLocationAllowed: false))
            {
                notifier.ShowSweepBlocked(CollectScope.Auto);
            }
        }

        public void UpdateTicked()
        {
            if (scanCache.ConsumeJustRefreshed())
                OnLocationContentChanged();

            TryPendingAutoStart();

            if (IsRunning && !CollectPolicy.IsLocationAllowed(config, Game1.currentLocation, currentSweepScope))
            {
                Cancel(returnToStart: false, suppressAutoUntilWarp: true);
                return;
            }

            if (!IsRunning)
                return;

            Farmer player = Game1.player;

            if (state == SweepState.Moving)
            {
                if (currentTarget != null && MovementHelper.IsCloseEnoughToCollect(player, currentTarget))
                {
                    hasLastPlayerPosition = true;
                    lastPlayerPosition = player.Position;
                    BeginSettling(usedSnap: false);
                    return;
                }

                if (player.controller == null)
                {
                    if (MovementHelper.ShouldUsePathfinding(config, player, ref lastPlayerPosition, hasLastPlayerPosition))
                        BeginSettling(usedSnap: false);
                    else
                        SnapAndSettle();

                    hasLastPlayerPosition = true;
                    lastPlayerPosition = player.Position;
                    return;
                }

                hasLastPlayerPosition = true;
                lastPlayerPosition = player.Position;
                return;
            }

            if (state == SweepState.AwaitingDebris)
            {
                if (currentTarget == null)
                {
                    AdvanceToNextTarget();
                    return;
                }

                player.controller = null;
                DebrisPickupHelper.CollectNearTile(Game1.currentLocation, player, currentTarget.Tile);
                debrisPickupTicks--;

                if (debrisPickupTicks <= 0)
                {
                    MovementHelper.ReleasePlayerControlIfNeeded(player);
                    currentTarget = null;
                    AdvanceToNextTarget();
                }

                hasLastPlayerPosition = true;
                lastPlayerPosition = player.Position;
                return;
            }

            if (state == SweepState.Settling)
            {
                if (currentTarget == null)
                {
                    AdvanceToNextTarget();
                    return;
                }

                CollectionHelper.PreparePlayerForTarget(Game1.currentLocation, player, currentTarget);
                settleAttempts++;

                if (MovementHelper.IsCloseEnoughToCollect(player, currentTarget))
                {
                    settleCounter--;
                    if (settleCounter <= 0)
                        CollectCurrent();

                    hasLastPlayerPosition = true;
                    lastPlayerPosition = player.Position;
                    return;
                }

                if (settleAttempts >= MovementHelper.GetMaxSettleTicks())
                {
                    SnapAndSettle();
                    CollectCurrent();
                }

                hasLastPlayerPosition = true;
                lastPlayerPosition = player.Position;
                return;
            }
        }

        private void ResetRunState()
        {
            queue.Clear();
            skippedTargets.Clear();
            collectedCount = 0;
            currentTarget = null;
            settleCounter = 0;
            settleAttempts = 0;
            debrisPickupTicks = 0;
            usedSnapForCurrent = false;
            hasLastPlayerPosition = false;
            sweepRadiusTiles = null;
        }

        private bool BuildQueue()
        {
            GameLocation location = Game1.currentLocation;
            Farmer player = Game1.player;

            IReadOnlyList<ForageTarget> targets = sweepRadiusTiles.HasValue
                ? scanCache.GetTargetsInRadius(location, player, sweepRadiusTiles.Value)
                : scanCache.GetAllTargets(location, player);

            foreach (ForageTarget target in targets)
            {
                if (!ForageTargetFilters.IsEnabledForCollect(config, target.Type, currentSweepScope, location))
                    continue;

                if (target.SkipReason == SkipReason.Unreachable
                    || target.SkipReason == SkipReason.NpcWitness)
                {
                    skippedTargets.Add(target);
                    continue;
                }

                if (!ToolHelper.HasRequiredTool(player, target.RequiredTool))
                {
                    target.SkipReason = SkipReason.MissingTool;
                    skippedTargets.Add(target);
                    notifier.ShowMissingTool(target.RequiredTool);
                    continue;
                }

                queue.Add(target);
            }

            return queue.Count > 0;
        }

        private void AdvanceToNextTarget()
        {
            while (queue.Count > 0)
            {
                currentTarget = queue[0];
                queue.RemoveAt(0);
                settleAttempts = 0;

                if (!IsTargetStillValid(currentTarget))
                    continue;

                if (!ForageTargetFilters.IsEnabledForCollect(
                    config,
                    currentTarget.Type,
                    currentSweepScope,
                    Game1.currentLocation))
                {
                    continue;
                }

                if (MovementHelper.IsCloseEnoughToCollect(Game1.player, currentTarget))
                {
                    BeginSettling(usedSnap: false);
                    return;
                }

                if (MovementHelper.ShouldUsePathfinding(config, Game1.player, ref lastPlayerPosition, hasLastPlayerPosition))
                    MoveToTarget(currentTarget);
                else
                    SnapAndSettle();

                return;
            }

            Finish();
        }

        private void MoveToTarget(ForageTarget target)
        {
            state = SweepState.Moving;
            usedSnapForCurrent = false;
            Vector2 stand = MovementHelper.GetStandOrTargetTile(target);
            Point end = new((int)stand.X, (int)stand.Y);

            Game1.player.controller = new PathFindController(
                Game1.player,
                Game1.currentLocation,
                end,
                -1,
                OnArrived);
        }

        private void OnArrived(Character character, GameLocation location)
        {
            if (state != SweepState.Moving)
                return;

            BeginSettling(usedSnap: false);
        }

        private void SnapAndSettle()
        {
            if (currentTarget == null)
            {
                AdvanceToNextTarget();
                return;
            }

            Farmer player = Game1.player;
            GameLocation location = Game1.currentLocation;
            Vector2 stand = MovementHelper.GetStandOrTargetTile(currentTarget);
            bool usePathfinding = MovementHelper.ShouldUsePathfinding(config, player, ref lastPlayerPosition, hasLastPlayerPosition);

            if (usePathfinding)
            {
                bool unreachable;
                if (currentTarget.Type == ForageType.Panning)
                {
                    unreachable = stand == Vector2.Zero
                        || !PanningHelper.IsValidPanStand(location, stand, currentTarget.Tile, player)
                        || !WalkabilityHelper.GetReachableTiles(location, player.Tile).Contains(stand);
                }
                else
                {
                    unreachable = !WalkabilityHelper.IsReachable(location, player.Tile, stand);
                }

                if (unreachable)
                {
                    if (config.ShowTargetLines)
                    {
                        currentTarget.SkipReason = SkipReason.Unreachable;
                        skippedTargets.Add(currentTarget);
                    }

                    currentTarget = null;
                    AdvanceToNextTarget();
                    return;
                }
            }

            if (currentTarget.Type == ForageType.Bush && currentTarget.Source is Bush bush)
                BushHelper.SnapForBush(location, player, bush, stand, usePathfinding);
            else if (currentTarget.Type == ForageType.Panning)
                PanningHelper.TryMoveToPanStand(location, player, currentTarget);
            else
                MovementHelper.SnapToStandTile(player, stand, usePathfinding);
            BeginSettling(usedSnap: true);
        }

        private void BeginSettling(bool usedSnap)
        {
            if (currentTarget == null)
            {
                AdvanceToNextTarget();
                return;
            }

            Game1.player.controller = null;
            MovementHelper.ReleasePlayerControlIfNeeded(Game1.player);
            usedSnapForCurrent = usedSnap;
            state = SweepState.Settling;
            settleCounter = MovementHelper.GetSettleTicks(usedSnap);
            settleAttempts = 0;
        }

        private void CollectCurrent()
        {
            if (currentTarget == null)
            {
                AdvanceToNextTarget();
                return;
            }

            state = SweepState.Collecting;

            GameLocation location = Game1.currentLocation;
            Farmer player = Game1.player;
            ForageTarget target = currentTarget;

            if (!IsTargetStillValid(target))
            {
                currentTarget = null;
                AdvanceToNextTarget();
                return;
            }

            if (!ForageTargetFilters.IsEnabledForCollect(config, target.Type, currentSweepScope, location))
            {
                currentTarget = null;
                AdvanceToNextTarget();
                return;
            }

            CollectionHelper.PreparePlayerForTarget(location, player, target);

            bool usePathfinding = MovementHelper.ShouldUsePathfinding(config, player, ref lastPlayerPosition, hasLastPlayerPosition);
            CollectResult result = CollectResult.Failed;
            for (int attempt = 0; attempt < 3; attempt++)
            {
                if (!MovementHelper.IsCloseEnoughToCollect(player, target))
                {
                    if (target.Type == ForageType.Panning)
                    {
                        if (!PanningHelper.TryMoveToPanStand(location, player, target))
                            continue;
                    }
                    else if (target.Type == ForageType.Bush && target.Source is Bush bush)
                    {
                        Vector2 stand = MovementHelper.GetStandOrTargetTile(target);
                        if (!BushHelper.TrySnapForInteraction(location, player, bush, stand, usePathfinding))
                            continue;
                    }
                    else
                    {
                        Vector2 stand = MovementHelper.GetStandOrTargetTile(target);
                        if (!MovementHelper.TrySnapToStandTile(location, player, stand, usePathfinding))
                            continue;
                    }
                }

                CollectionHelper.PreparePlayerForTarget(location, player, target);
                result = collectionService.TryCollect(location, player, target);

                if (result == CollectResult.Success)
                    break;
            }

            if (target.Type == ForageType.Panning)
                PanningHelper.ClearPanAnimationState(player);

            MovementHelper.ReleasePlayerControlIfNeeded(player);

            if (result == CollectResult.Success)
                collectedCount++;
            else if (config.ShowTargetLines)
                skippedTargets.Add(target);

            if (result == CollectResult.Success && ForageItemHelper.DropsOnGround(target))
            {
                state = SweepState.AwaitingDebris;
                debrisPickupTicks = 15;
                return;
            }

            currentTarget = null;
            AdvanceToNextTarget();
        }

        private static bool IsTargetStillValid(ForageTarget target)
        {
            GameLocation location = Game1.currentLocation;

            return target.Type switch
            {
                ForageType.Ground or ForageType.ArtifactSpot => location.objects.ContainsKey(target.Tile),
                ForageType.ForageCrop => ForageCropHelper.TryGetAt(location, target.Tile, out _),
                ForageType.Panning => PanningHelper.IsActivePanTile(location, target.Tile),
                ForageType.Bush => BushHelper.TryGetAt(location, target.Tile, out Bush bush) && BushHelper.IsHarvestable(bush),
                ForageType.CrabPot => CrabPotHelper.TryGetAt(location, target.Tile, out _) && CrabPotHelper.IsHarvestable(location.objects[target.Tile]),
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

        private void Finish()
        {
            Farmer player = Game1.player;
            player.controller = null;
            MovementHelper.ReleasePlayerControl(player);
            state = SweepState.Done;
            int experienceGained = ExperienceTracker.GetGainedSince(player, experienceAtSweepStart);
            notifier.ShowSweepComplete(collectedCount, experienceGained);

            if (config.ReturnToStartAfterSweep)
                ReturnToStartPosition();

            sweepLocationName = null;
            MovementHelper.ReleasePlayerControl(player);
            state = SweepState.Idle;
        }

        private void ReturnToStartPosition()
        {
            if (sweepLocationName == null
                || Game1.currentLocation.NameOrUniqueName != sweepLocationName)
            {
                return;
            }

            Farmer player = Game1.player;
            player.controller = null;
            MovementHelper.TrySnapToStandTile(Game1.currentLocation, player, returnTile);
            MovementHelper.ReleasePlayerControlIfNeeded(player);
        }

        private void TryPendingAutoStart()
        {
            if (!pendingAutoStart)
                return;

            if (IsRunning)
            {
                pendingAutoStart = false;
                return;
            }

            if (!Context.IsWorldReady)
                return;

            if (!AutomationGate.CanAutomate(Game1.player))
                return;

            if (pendingAutoStartWarmupTicks > 0)
            {
                pendingAutoStartWarmupTicks--;
                return;
            }

            if (MovementHelper.IsRidingHorse(Game1.player))
                return;

            pendingAutoStart = false;
            StartAutomatic();
        }

        private bool HasCollectableTargets(CollectScope scope, bool requireLocationAllowed = true)
        {
            if (!Context.IsWorldReady)
                return false;

            GameLocation location = Game1.currentLocation;

            if (requireLocationAllowed && !CollectPolicy.IsLocationAllowed(config, location, scope))
                return false;

            Farmer player = Game1.player;

            IReadOnlyList<ForageTarget> targets = scanCache.GetAllTargets(location, player);

            foreach (ForageTarget target in targets)
            {
                if (!IsCollectableTarget(config, target, scope, location, requireLocationAllowed))
                    continue;

                return true;
            }

            return false;
        }

        private static bool IsCollectableTarget(
            ModConfig config,
            ForageTarget target,
            CollectScope scope,
            GameLocation location,
            bool requireLocationAllowed)
        {
            if (requireLocationAllowed)
            {
                if (!ForageTargetFilters.IsEnabledForCollect(config, target.Type, scope, location))
                    return false;
            }
            else if (!CollectPolicy.IsTypeAllowed(config, target.Type, scope))
            {
                return false;
            }

            if (target.SkipReason == SkipReason.Unreachable)
                return false;

            if (!ToolHelper.HasRequiredTool(Game1.player, target.RequiredTool))
                return false;

            return true;
        }
    }

}
