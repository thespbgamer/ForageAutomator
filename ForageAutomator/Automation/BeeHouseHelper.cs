using StardewValley;
using SObject = StardewValley.Object;

namespace ForageAutomator.Automation
{
    internal static class BeeHouseHelper
    {
        private const string BeeHouseId = "(BC)10";

        public static bool IsBeeHouse(SObject obj)
        {
            return obj.QualifiedItemId == BeeHouseId || obj.ItemId == "10" && obj.bigCraftable.Value;
        }

        public static bool IsHarvestable(SObject obj)
        {
            return IsBeeHouse(obj) && PlacedObjectHelper.IsOutputReady(obj);
        }

        public static bool TryGetAt(GameLocation location, Microsoft.Xna.Framework.Vector2 tile, out SObject? beeHouse)
        {
            if (location.objects.TryGetValue(tile, out SObject? obj) && IsHarvestable(obj))
            {
                beeHouse = obj;
                return true;
            }

            beeHouse = null;
            return false;
        }
    }

}
