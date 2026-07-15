using ForageAutomator.Automation;
using Microsoft.Xna.Framework;
using StardewValley;

namespace ForageAutomator.Collectors
{
    internal sealed class GarbageCanCollector : IForageCollector
    {
        public bool CanCollect(ForageTarget target) => target.Type == ForageType.GarbageCan;

        public CollectResult TryCollect(GameLocation location, Farmer player, ForageTarget target)
        {
            if (!ToolHelper.HasInventorySpace(player))
                return CollectResult.InventoryFull;

            if (target.Source is not GarbageCanInfo can)
                return CollectResult.Skipped;

            if (!GarbageCanHelper.CanCheckToday(can.Id))
                return CollectResult.Skipped;

            if (!GarbageCanHelper.TryLoot(location, can, player, blockWhenWitnessed: false))
                return CollectResult.Skipped;

            DebrisPickupHelper.CollectNearTile(location, player, can.Tile);
            return CollectResult.Success;
        }
    }

}
