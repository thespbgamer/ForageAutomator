using System;
using StardewValley;

namespace ForageAutomator.Automation
{
    internal static class ExperienceTracker
    {
        public static int CaptureTotal(Farmer player)
        {
            int total = 0;
            foreach (int xp in player.experiencePoints)
                total += xp;

            return total;
        }

        public static int GetGainedSince(Farmer player, int startTotal)
        {
            return Math.Max(0, CaptureTotal(player) - startTotal);
        }
    }

}
