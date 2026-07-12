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
        private readonly ForageTargetScanner scanner;
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
        private const int AutoStartWarmupTicks = 15;
        private int? sweepRadiusTiles;
        private Vector2 lastPlayerPosition;
        private bool hasLastPlayerPosition;

        public bool IsRunning => state != SweepState.Idle && state != SweepState.Done;
        public SweepState State => state;
        public IReadOnlyList<ForageTarget> SkippedTargets => skippedTargets;
        public int QueueCount => queue.Count;

        public void ClearSkippedTargets()
        {
            skippedTargets.Clear();
        }

        public ForageRunController(ModConfig config, HudNotifier notifier)
        {
            this.config = config;
            this.notifier = notifier;
            scanner = new ForageTargetScanner();
            collectionService = new ForageCollectionService(notifier);
        }

        public void StartWholeMap()
        {
            StartSweep(maxRadius: null);
        }

        public void StartRange()
        {
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

            if (!Context.IsPlayerFree && Game1.player.controller == null)
                return;

            BeginSweep(maxRadius);
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

            if (!Context.IsPlayerFree && Game1.player.controller == null)
                return;

            BeginSweep(maxRadius: null);
        }

        private void BeginSweep(int? maxRadius)
        {
            sweepRadiusTiles = maxRadius;
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
            if (!config.AutoCollectWholeMap)
                return;

            pendingAutoStart = true;
            pendingAutoStartWarmupTicks = AutoStartWarmupTicks;
        }

        public void Cancel()
        {
            Cancel(returnToStart: config.ReturnToStartAfterSweep);
        }

        public void Cancel(bool returnToStart)
        {
            pendingAutoStart = false;
            pendingAutoStartWarmupTicks = 0;

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
            WalkabilityHelper.InvalidateReachabilityCache();

            if (IsRunning)
                Cancel(returnToStart: false);
        }

        public void UpdateTicked()
        {
            TryPendingAutoStart();

            if (!IsRunning)
                return;

            Farmer player = Game1.player;
            bool highSpeed = MovementHelper.IsHighSpeedMovement(player, ref lastPlayerPosition, hasLastPlayerPosition);
            hasLastPlayerPosition = true;
            lastPlayerPosition = player.Position;

            if (state == SweepState.Moving)
            {
                if (currentTarget != null && MovementHelper.IsCloseEnoughToCollect(player, currentTarget))
                {
                    BeginSettling(usedSnap: false);
                    return;
                }

                if (player.controller == null)
                {
                    if (config.UsePathfinding && !highSpeed)
                        BeginSettling(usedSnap: false);
                    else
                        SnapAndSettle();
                    return;
                }

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

                return;
            }

            if (state == SweepState.Settling)
            {
                if (currentTarget == null)
                {
                    AdvanceToNextTarget();
                    return;
                }

                CollectionHelper.PreparePlayer(player, BushHelper.GetTargetFaceTile(Game1.currentLocation, player, currentTarget));
                settleAttempts++;

                if (MovementHelper.IsCloseEnoughToCollect(player, currentTarget))
                {
                    settleCounter--;
                    if (settleCounter <= 0)
                        CollectCurrent();
                    return;
                }

                if (settleAttempts >= MovementHelper.GetMaxSettleTicks())
                {
                    SnapAndSettle();
                    CollectCurrent();
                    return;
                }

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
                ? scanner.Scan(location, player, sweepRadiusTiles.Value)
                : scanner.Scan(location, player);
            scanner.ApplyReachability(location, player, targets);

            foreach (ForageTarget target in targets)
            {
                if (!ForageTargetFilters.IsEnabledForCollect(config, target.Type))
                    continue;

                if (target.SkipReason == SkipReason.Unreachable)
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

                if (MovementHelper.IsCloseEnoughToCollect(Game1.player, currentTarget))
                {
                    BeginSettling(usedSnap: false);
                    return;
                }

                if (config.UsePathfinding && !MovementHelper.IsHighSpeedMovement(Game1.player, ref lastPlayerPosition, hasLastPlayerPosition))
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

            Vector2 stand = MovementHelper.GetStandOrTargetTile(currentTarget);
            if (!WalkabilityHelper.IsReachable(Game1.currentLocation, Game1.player.Tile, stand))
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

            if (currentTarget.Type == ForageType.Bush && currentTarget.Source is Bush bush)
                BushHelper.SnapForBush(Game1.currentLocation, Game1.player, bush, stand);
            else
                MovementHelper.SnapToStandTile(Game1.player, stand);
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

            CollectionHelper.PreparePlayer(player, BushHelper.GetTargetFaceTile(location, player, target));

            CollectResult result = CollectResult.Failed;
            for (int attempt = 0; attempt < 3; attempt++)
            {
                if (!MovementHelper.IsCloseEnoughToCollect(player, target))
                {
                    Vector2 stand = MovementHelper.GetStandOrTargetTile(target);
                    if (target.Type == ForageType.Bush && target.Source is Bush bush)
                    {
                        if (!BushHelper.TrySnapForInteraction(location, player, bush, stand))
                            continue;
                    }
                    else if (!MovementHelper.TrySnapToStandTile(location, player, stand))
                    {
                        continue;
                    }
                }

                CollectionHelper.PreparePlayer(player, BushHelper.GetTargetFaceTile(location, player, target));
                result = collectionService.TryCollect(location, player, target);

                if (result == CollectResult.Success)
                    break;
            }

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
                ForageType.Panning => (location.orePanPoint?.Value ?? Point.Zero) != Point.Zero,
                ForageType.Bush => BushHelper.TryGetAt(location, target.Tile, out Bush bush) && BushHelper.IsHarvestable(bush),
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

            if (pendingAutoStartWarmupTicks > 0)
            {
                pendingAutoStartWarmupTicks--;
                return;
            }

            if (MovementHelper.IsRidingHorse(Game1.player))
                return;

            if (!Context.IsPlayerFree && Game1.player.controller == null)
                return;

            pendingAutoStart = false;
            StartAutomatic();
        }
    }

}
