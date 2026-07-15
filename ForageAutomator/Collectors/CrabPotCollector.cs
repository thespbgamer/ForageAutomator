using ForageAutomator.Automation;
using Microsoft.Xna.Framework;
using StardewValley;
using SObject = StardewValley.Object;

namespace ForageAutomator.Collectors
{
    internal sealed class CrabPotCollector : IForageCollector
    {
        public bool CanCollect(ForageTarget target) => target.Type == ForageType.CrabPot;

        public CollectResult TryCollect(GameLocation location, Farmer player, ForageTarget target)
        {
            if (!ToolHelper.HasInventorySpace(player))
                return CollectResult.InventoryFull;

            if (target.Source is not SObject pot || !CrabPotHelper.IsHarvestable(pot))
                return CollectResult.Skipped;

            if (!PlacedObjectHelper.CanInteractFrom(player, target.Tile))
                return CollectResult.Failed;

            return PlacedObjectHelper.TryHarvestObject(location, player, pot, target.Tile)
                ? CollectResult.Success
                : CollectResult.Failed;
        }
    }

}
