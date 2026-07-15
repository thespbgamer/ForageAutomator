using ForageAutomator.Automation;
using Microsoft.Xna.Framework;
using StardewValley;
using SObject = StardewValley.Object;

namespace ForageAutomator.Collectors
{
    internal sealed class BeeHouseCollector : IForageCollector
    {
        public bool CanCollect(ForageTarget target) => target.Type == ForageType.BeeHouse;

        public CollectResult TryCollect(GameLocation location, Farmer player, ForageTarget target)
        {
            if (!ToolHelper.HasInventorySpace(player))
                return CollectResult.InventoryFull;

            if (target.Source is not SObject beeHouse || !BeeHouseHelper.IsHarvestable(beeHouse))
                return CollectResult.Skipped;

            return PlacedObjectHelper.TryHarvestObject(location, player, beeHouse, target.Tile)
                ? CollectResult.Success
                : CollectResult.Failed;
        }
    }

}
