using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Locations;

namespace ForageAutomator
{
    internal static class LocationCatalog
    {
        private static IReadOnlyList<string> locationIds = Array.Empty<string>();

        public static IReadOnlyList<string> LocationIds => locationIds;

        public static void Initialize(IModHelper helper)
        {
            try
            {
                Dictionary<string, LocationData> data =
                    helper.GameContent.Load<Dictionary<string, LocationData>>("Data/Locations");

                locationIds = data
                    .Where(pair => ForageLocationFilter.IsForageCapable(pair.Key, pair.Value, helper))
                    .Select(pair => pair.Key)
                    .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
                    .ToArray();
            }
            catch
            {
                locationIds = VanillaLocationIds.ForageCapable
                    .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
                    .ToArray();
            }
        }

        public static string GetDisplayName(string locationId)
        {
            if (Context.IsWorldReady)
            {
                GameLocation? location = Game1.getLocationFromName(locationId);
                if (location != null)
                    return location.DisplayName;
            }

            return locationId.Replace('_', ' ');
        }
    }

}
