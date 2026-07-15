using ForageAutomator.Automation;
using Microsoft.Xna.Framework;
using StardewValley;
using SObject = StardewValley.Object;

namespace ForageAutomator.Collectors
{
    internal sealed class TapperCollector : IForageCollector
    {
        public bool CanCollect(ForageTarget target) => target.Type == ForageType.Tapper;

        public CollectResult TryCollect(GameLocation location, Farmer player, ForageTarget target)
        {
            if (!ToolHelper.HasInventorySpace(player))
                return CollectResult.InventoryFull;

            if (target.Source is not SObject tapper || !TapperHelper.IsHarvestable(tapper))
                return CollectResult.Skipped;

            return PlacedObjectHelper.TryHarvestObject(location, player, tapper, target.Tile)
                ? CollectResult.Success
                : CollectResult.Failed;
        }
    }

}
