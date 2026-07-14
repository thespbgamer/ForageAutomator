using System;
using ForageAutomator.Automation;
using Microsoft.Xna.Framework;
using StardewValley;
using SObject = StardewValley.Object;

namespace ForageAutomator.Collectors
{
    internal sealed class GroundObjectCollector : IForageCollector
    {
        public bool CanCollect(ForageTarget target) => target.Type == ForageType.Ground;

        public CollectResult TryCollect(GameLocation location, Farmer player, ForageTarget target)
        {
            if (!ToolHelper.HasInventorySpace(player))
                return CollectResult.InventoryFull;

            if (target.Source is not SObject obj)
                return CollectResult.Failed;

            if (!location.objects.ContainsKey(target.Tile))
                return CollectResult.Skipped;

            if (ForageTargetScanner.IsArtifactSpotObject(obj))
                return CollectResult.Skipped;

            if (!CollectionHelper.CanInteractWith(player, target.Tile))
                return CollectResult.Failed;

            Random random = Utility.CreateDaySaveRandom(target.Tile.X, target.Tile.Y * 777f);
            Item pickup = obj.getOne();

            if (player.professions.Contains(Farmer.botanist) && pickup is SObject pickupObj && pickupObj.isForage())
                pickupObj.Quality = 4;
            else if (pickup is SObject forageObj && forageObj.isForage())
                ForageItemHelper.ApplyForageQuality(player, forageObj, random);

            if (player.addItemToInventory(pickup) != null)
                return CollectResult.InventoryFull;

            ForageRewardHelper.GrantGroundPickup(player, obj, pickup, random, target.Tile, location);

            if (location.removeObject(target.Tile, false) == null)
                location.objects.Remove(target.Tile);

            Game1.playSound("harvest");
            return CollectResult.Success;
        }
    }

}
