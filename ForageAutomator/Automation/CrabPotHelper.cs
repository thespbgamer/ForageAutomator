using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace ForageAutomator.Automation
{
    internal static class CrabPotHelper
    {
        private const string CrabPotId = "(O)710";

        public static bool IsCrabPot(SObject obj)
        {
            return obj is CrabPot || obj.QualifiedItemId == CrabPotId || obj.ItemId == "710";
        }

        public static bool IsHarvestable(SObject obj)
        {
            return IsCrabPot(obj) && obj.heldObject.Value != null && obj.MinutesUntilReady <= 0;
        }

        public static bool TryGetAt(GameLocation location, Vector2 tile, out SObject? pot)
        {
            if (location.objects.TryGetValue(tile, out SObject? obj) && IsCrabPot(obj))
            {
                pot = obj;
                return true;
            }

            pot = null;
            return false;
        }
    }

}
