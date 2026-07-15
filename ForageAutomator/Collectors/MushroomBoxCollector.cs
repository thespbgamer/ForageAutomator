using ForageAutomator.Automation;
using Microsoft.Xna.Framework;
using StardewValley;
using SObject = StardewValley.Object;

namespace ForageAutomator.Collectors
{
    internal sealed class MushroomBoxCollector : IForageCollector
    {
        public bool CanCollect(ForageTarget target) => target.Type == ForageType.MushroomBox;

        public CollectResult TryCollect(GameLocation location, Farmer player, ForageTarget target)
        {
            if (!ToolHelper.HasInventorySpace(player))
                return CollectResult.InventoryFull;

            if (target.Source is not SObject producer || !MushroomBoxHelper.IsHarvestable(producer))
                return CollectResult.Skipped;

            return PlacedObjectHelper.TryHarvestObject(location, player, producer, target.Tile)
                ? CollectResult.Success
                : CollectResult.Failed;
        }
    }

}
