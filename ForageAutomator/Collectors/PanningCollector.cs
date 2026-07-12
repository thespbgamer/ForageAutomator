using System.Collections.Generic;
using ForageAutomator.Automation;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Tools;

namespace ForageAutomator.Collectors
{
    internal sealed class PanningCollector : IForageCollector
    {
        public bool CanCollect(ForageTarget target) => target.Type == ForageType.Panning;

        public CollectResult TryCollect(GameLocation location, Farmer player, ForageTarget target)
        {
            if (!ToolHelper.HasCopperPan(player))
                return CollectResult.MissingTool;

            if (!ToolHelper.HasInventorySpace(player))
                return CollectResult.InventoryFull;

            if (!CollectionHelper.CanInteractWith(player, target.Tile))
                return CollectResult.Failed;

            CollectionHelper.PreparePlayer(player, target.Tile);

            Point panPoint = location.orePanPoint?.Value ?? Point.Zero;
            if (panPoint == Point.Zero)
                return CollectResult.Skipped;

            Pan? pan = ToolHelper.FindCopperPan(player);
            if (pan == null)
                return CollectResult.MissingTool;

            IList<Item?> items = pan.getPanItems(location, player);
            if (items == null || items.Count == 0)
                return CollectResult.Failed;

            Vector2 panTile = panPoint.ToVector2();

            foreach (Item? item in items)
            {
                if (item == null)
                    continue;

                if (player.addItemToInventory(item) != null)
                    return CollectResult.InventoryFull;
            }

            location.localSound("coin", panTile * Game1.tileSize);
            location.orePanPoint!.Value = Point.Zero;
            location.performOrePanTenMinuteUpdate(Game1.random);

            return CollectResult.Success;
        }
    }

}
