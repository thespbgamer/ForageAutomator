using Microsoft.Xna.Framework;
using StardewValley;
using SObject = StardewValley.Object;

namespace ForageAutomator.Automation
{
    internal static class PlacedObjectHelper
    {
        public static bool IsOutputReady(SObject obj)
        {
            return obj.heldObject.Value != null && obj.MinutesUntilReady <= 0;
        }

        public static bool CanInteractFrom(Farmer player, Vector2 tile)
        {
            return CollectionHelper.CanInteractWith(player, tile);
        }

        public static bool TryHarvestObject(
            GameLocation location,
            Farmer player,
            SObject obj,
            Vector2 tile)
        {
            if (!CanInteractFrom(player, tile))
                return false;

            CollectionHelper.PreparePlayer(player, tile);
            return obj.checkForAction(player, false);
        }
    }

}
