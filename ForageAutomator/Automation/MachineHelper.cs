using StardewValley;
using SObject = StardewValley.Object;

namespace ForageAutomator.Automation
{
    internal static class MachineHelper
    {
        public static bool IsProcessingMachine(SObject obj)
        {
            if (obj.GetMachineData() is null)
                return false;

            if (obj.IsTapper())
                return false;

            if (BeeHouseHelper.IsBeeHouse(obj))
                return false;

            if (MushroomBoxHelper.IsMushroomProducer(obj))
                return false;

            if (CrabPotHelper.IsCrabPot(obj))
                return false;

            return true;
        }

        public static bool IsHarvestable(SObject obj)
        {
            return IsProcessingMachine(obj) && PlacedObjectHelper.IsOutputReady(obj);
        }

        public static bool TryGetAt(GameLocation location, Microsoft.Xna.Framework.Vector2 tile, out SObject? machine)
        {
            if (location.objects.TryGetValue(tile, out SObject? obj) && IsHarvestable(obj))
            {
                machine = obj;
                return true;
            }

            machine = null;
            return false;
        }
    }

}
