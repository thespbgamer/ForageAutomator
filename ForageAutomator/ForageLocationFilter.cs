using System;
using System.Collections.Generic;
using StardewModdingAPI;
using StardewValley.GameData.Locations;
using xTile;

namespace ForageAutomator
{
    internal static class ForageLocationFilter
    {
        private static readonly HashSet<string> AlwaysInclude = new(StringComparer.OrdinalIgnoreCase)
        {
            "Greenhouse",
            "FarmCave"
        };

        private static readonly HashSet<string> AlwaysExclude = new(StringComparer.OrdinalIgnoreCase)
        {
            "Default"
        };

        public static bool IsForageCapable(string locationId, LocationData? data, IModHelper helper)
        {
            if (AlwaysExclude.Contains(locationId))
                return false;

            if (AlwaysInclude.Contains(locationId))
                return true;

            if (data == null)
                return MineLocationIds.IsMine(locationId);

            if (MineLocationIds.IsMine(locationId, data))
                return true;

            if (data.Forage?.Count > 0)
                return true;

            string? mapPath = data.CreateOnLoad?.MapPath;
            if (!string.IsNullOrWhiteSpace(mapPath))
            {
                try
                {
                    Map map = helper.GameContent.Load<Map>(mapPath);
                    if (map.Properties.ContainsKey("Outdoors"))
                        return true;
                }
                catch
                {
                    // Fall through to other heuristics.
                }
            }

            return HasForageSpawnConfiguration(data);
        }

        private static bool HasForageSpawnConfiguration(LocationData data)
        {
            return data.MaxSpawnedForageAtOnce > 0
                || data.MinDailyForageSpawn > 0
                || data.MaxDailyForageSpawn > 0;
        }
    }

}
