using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

namespace ForageAutomator.Automation
{
    internal static class AutomationGate
    {
        public static bool CanAutomate(Farmer player)
        {
            if (!Context.IsWorldReady || !Context.IsPlayerFree)
                return false;

            if (Game1.eventUp || Game1.freezeControls || Game1.dialogueUp)
                return false;

            if (Game1.currentMinigame != null || Game1.activeClickableMenu != null)
                return false;

            if (Game1.gameMode != 3)
                return false;

            if (Game1.locationRequest != null || Game1.IsFading())
                return false;

            if (IsBusAnimating())
                return false;

            if (player.controller != null)
                return false;

            return true;
        }

        private static bool IsBusAnimating()
        {
            return Game1.currentLocation switch
            {
                BusStop stop => stop.drivingOff || stop.drivingBack,
                Desert desert => desert.drivingOff || desert.drivingBack,
                _ => false
            };
        }
    }
}
