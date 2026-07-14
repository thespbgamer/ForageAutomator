using ForageAutomator.Automation;
using StardewValley;
using SObject = StardewValley.Object;

namespace ForageAutomator.Collectors
{
    internal sealed class ArtifactSpotCollector : IForageCollector
    {
        public bool CanCollect(ForageTarget target) => target.Type == ForageType.ArtifactSpot;

        public CollectResult TryCollect(GameLocation location, Farmer player, ForageTarget target)
        {
            if (!ToolHelper.HasHoe(player))
                return CollectResult.MissingTool;

            if (!ToolHelper.HasInventorySpace(player))
                return CollectResult.InventoryFull;

            if (target.Source is not SObject obj)
                return CollectResult.Failed;

            if (!location.objects.ContainsKey(target.Tile))
                return CollectResult.Skipped;

            if (!CollectionHelper.CanInteractWith(player, target.Tile))
                return CollectResult.Failed;

            if (!ToolHelper.UseHoeOnObject(location, player, obj, target.Tile))
                return CollectResult.MissingTool;

            int pickedUp = DebrisPickupHelper.CollectNearTile(location, player, target.Tile);
            bool spotRemoved = !location.objects.ContainsKey(target.Tile);

            if (spotRemoved)
            {
                if (pickedUp > 0)
                    Game1.playSound("coin");
                else
                    Game1.playSound("hoeHit");

                return CollectResult.Success;
            }

            if (pickedUp > 0)
            {
                location.objects.Remove(target.Tile);
                Game1.playSound("coin");
                return CollectResult.Success;
            }

            return CollectResult.Failed;
        }
    }

}
