using ForageAutomator.Automation;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace ForageAutomator.Collectors
{
    internal sealed class FruitTreeCollector : IForageCollector
    {
        public bool CanCollect(ForageTarget target) => target.Type == ForageType.FruitTree;

        public CollectResult TryCollect(GameLocation location, Farmer player, ForageTarget target)
        {
            if (!ToolHelper.HasInventorySpace(player))
                return CollectResult.InventoryFull;

            if (target.Source is not FruitTree tree || !FruitTreeHelper.IsHarvestable(tree))
                return CollectResult.Skipped;

            if (!CollectionHelper.CanInteractWith(player, target.Tile))
                return CollectResult.Failed;

            CollectionHelper.PreparePlayer(player, target.Tile);
            bool shook = tree.performUseAction(target.Tile);
            if (!shook)
                return CollectResult.Failed;

            DebrisPickupHelper.CollectForDebrisDrop(location, player, target.Type, target.Tile);
            return CollectResult.Success;
        }
    }

}
