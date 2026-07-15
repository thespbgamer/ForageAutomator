using StardewValley;
using SObject = StardewValley.Object;

namespace ForageAutomator.Automation
{
    internal static class MushroomBoxHelper
    {
        private const string MushroomBoxId = "(O)256";
        private const string MushroomLogId = "(BC)264";

        public static bool IsMushroomProducer(SObject obj)
        {
            if (obj.QualifiedItemId is MushroomBoxId or MushroomLogId)
                return true;

            return obj.ItemId is "256" or "264";
        }

        public static bool IsHarvestable(SObject obj)
        {
            return IsMushroomProducer(obj) && PlacedObjectHelper.IsOutputReady(obj);
        }

        public static bool TryGetAt(GameLocation location, Microsoft.Xna.Framework.Vector2 tile, out SObject? producer)
        {
            if (location.objects.TryGetValue(tile, out SObject? obj) && IsHarvestable(obj))
            {
                producer = obj;
                return true;
            }

            producer = null;
            return false;
        }
    }

}
