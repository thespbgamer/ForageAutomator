using System.Collections.Generic;
using ForageAutomator.Automation;
using ForageAutomator.Collectors;
using ForageAutomator.Notifications;
using StardewValley;

namespace ForageAutomator.Automation
{
    internal sealed class ForageCollectionService
    {
        private readonly IReadOnlyList<IForageCollector> collectors;
        private readonly HudNotifier notifier;

        public ForageCollectionService(HudNotifier notifier)
        {
            this.notifier = notifier;
            collectors = new IForageCollector[]
            {
                new GroundObjectCollector(),
                new ForageCropCollector(),
                new BushCollector(),
                new ArtifactSpotCollector(),
                new PanningCollector()
            };
        }

        public CollectResult TryCollect(GameLocation location, Farmer player, ForageTarget target)
        {
            if (!ToolHelper.HasRequiredTool(player, target.RequiredTool))
            {
                target.SkipReason = SkipReason.MissingTool;
                notifier.ShowMissingTool(target.RequiredTool);
                return CollectResult.MissingTool;
            }

            if (!ToolHelper.HasInventorySpace(player))
            {
                target.SkipReason = SkipReason.InventoryFull;
                notifier.ShowInventoryFull();
                return CollectResult.InventoryFull;
            }

            foreach (IForageCollector collector in collectors)
            {
                if (!collector.CanCollect(target))
                    continue;

                CollectResult result = collector.TryCollect(location, player, target);
                HandleResult(target, result);
                return result;
            }

            return CollectResult.Failed;
        }

        private void HandleResult(ForageTarget target, CollectResult result)
        {
            target.SkipReason = result switch
            {
                CollectResult.InventoryFull => SkipReason.InventoryFull,
                CollectResult.MissingTool => SkipReason.MissingTool,
                CollectResult.Success => SkipReason.None,
                _ => target.SkipReason
            };
        }

        public bool CanAttempt(GameLocation location, Farmer player, ForageTarget target)
        {
            if (target.IsSkipped && target.SkipReason == SkipReason.Unreachable)
                return false;

            if (!ToolHelper.HasRequiredTool(player, target.RequiredTool))
            {
                target.SkipReason = SkipReason.MissingTool;
                return false;
            }

            return true;
        }
    }

}
