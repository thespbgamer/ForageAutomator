using ForageAutomator.Automation;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace ForageAutomator.Collectors
{
    internal sealed class BushCollector : IForageCollector
    {
        public bool CanCollect(ForageTarget target) => target.Type == ForageType.Bush;

        public CollectResult TryCollect(GameLocation location, Farmer player, ForageTarget target)
        {
            if (!ToolHelper.HasInventorySpace(player))
                return CollectResult.InventoryFull;

            if (target.Source is not Bush bush)
                return CollectResult.Failed;

            if (!BushHelper.IsHarvestable(bush))
                return CollectResult.Skipped;

            Vector2 preferredStand = MovementHelper.GetStandOrTargetTile(target);
            if (!BushHelper.PrepareForInteraction(location, player, bush)
                && !BushHelper.TrySnapForInteraction(location, player, bush, preferredStand))
                return CollectResult.Failed;

            bool wasHarvestable = bush.readyForHarvest();
            bush.performUseAction(bush.Tile);

            if (!wasHarvestable || BushHelper.IsHarvestable(bush))
                return CollectResult.Failed;

            int pickedUp = DebrisPickupHelper.CollectNearTile(location, player, bush.Tile);
            return pickedUp > 0 || !bush.isActionable()
                ? CollectResult.Success
                : CollectResult.Failed;
        }
    }

}
