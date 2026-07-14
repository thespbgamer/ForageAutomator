using ForageAutomator.Automation;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;

namespace ForageAutomator.Collectors
{
    internal sealed class ForageCropCollector : IForageCollector
    {
        public bool CanCollect(ForageTarget target) => target.Type == ForageType.ForageCrop;

        public CollectResult TryCollect(GameLocation location, Farmer player, ForageTarget target)
        {
            if (!ToolHelper.HasInventorySpace(player))
                return CollectResult.InventoryFull;

            if (target.Source is not HoeDirt hoeDirt || hoeDirt.crop == null)
                return CollectResult.Failed;

            if (!ForageCropHelper.IsHarvestable(hoeDirt))
                return CollectResult.Skipped;

            if (!CollectionHelper.CanInteractWith(player, target.Tile))
                return CollectResult.Failed;

            int x = (int)target.Tile.X;
            int y = (int)target.Tile.Y;
            Crop crop = hoeDirt.crop;

            if (ForageCropHelper.IsGinger(crop))
                return CollectGinger(location, player, target, hoeDirt, crop, x, y);

            if (!hoeDirt.readyForHarvest())
                return CollectResult.Skipped;

            CollectionHelper.PreparePlayer(player, target.Tile);

            if (!crop.harvest(x, y, hoeDirt))
                return ToolHelper.HasInventorySpace(player) ? CollectResult.Failed : CollectResult.InventoryFull;

            hoeDirt.destroyCrop(showAnimation: false);
            return CollectResult.Success;
        }

        private static CollectResult CollectGinger(GameLocation location, Farmer player, ForageTarget target, HoeDirt hoeDirt, Crop crop, int x, int y)
        {
            Hoe? hoe = ToolHelper.FindHoe(player);
            if (hoe == null)
                return CollectResult.MissingTool;

            CollectionHelper.PreparePlayer(player, target.Tile);

            int previousSlot = ToolHelper.SelectTool(player, hoe);
            try
            {
                hoe.lastUser = player;
                if (!crop.hitWithHoe(x, y, location, hoeDirt))
                    return CollectResult.Failed;

                hoeDirt.destroyCrop(showAnimation: false);
                DebrisPickupHelper.CollectNearTile(location, player, target.Tile);
                ForageRewardHelper.GrantGingerHarvest(player);
                Game1.playSound("harvest");
                return CollectResult.Success;
            }
            finally
            {
                ToolHelper.RestoreToolSlot(player, previousSlot);
            }
        }
    }

}
