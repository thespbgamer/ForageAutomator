using ForageAutomator.Automation;
using Microsoft.Xna.Framework;
using StardewValley;
using SObject = StardewValley.Object;

namespace ForageAutomator.Collectors
{
    internal sealed class MachineCollector : IForageCollector
    {
        public bool CanCollect(ForageTarget target) => target.Type == ForageType.Machine;

        public CollectResult TryCollect(GameLocation location, Farmer player, ForageTarget target)
        {
            if (!ToolHelper.HasInventorySpace(player))
                return CollectResult.InventoryFull;

            if (target.Source is not SObject machine || !MachineHelper.IsHarvestable(machine))
                return CollectResult.Skipped;

            return PlacedObjectHelper.TryHarvestObject(location, player, machine, target.Tile)
                ? CollectResult.Success
                : CollectResult.Failed;
        }
    }

}
