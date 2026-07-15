using StardewValley;
using SObject = StardewValley.Object;

namespace ForageAutomator.Automation
{
    internal static class TapperHelper
    {
        public static bool IsTapper(SObject obj) => obj.IsTapper();

        public static bool IsHarvestable(SObject obj)
        {
            return IsTapper(obj) && PlacedObjectHelper.IsOutputReady(obj);
        }

        public static bool TryGetAt(GameLocation location, Microsoft.Xna.Framework.Vector2 tile, out SObject? tapper)
        {
            if (location.objects.TryGetValue(tile, out SObject? obj) && IsHarvestable(obj))
            {
                tapper = obj;
                return true;
            }

            tapper = null;
            return false;
        }
    }

}
