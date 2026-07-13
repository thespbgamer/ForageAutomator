using System;
using System.Collections.Generic;
using StardewValley;
using StardewValley.GameData.Locations;
using StardewValley.Locations;

namespace ForageAutomator
{
    internal static class MineLocationIds
    {
        private const string UndergroundMinePrefix = "UndergroundMine";

        private static readonly HashSet<string> KnownIds = new(StringComparer.OrdinalIgnoreCase)
        {
            "Mine",
            "SkullCave",
            UndergroundMinePrefix,
            "MasteryCave",
            "VolcanoDungeon"
        };

        public static bool IsMine(string locationId, LocationData? data = null, GameLocation? location = null)
        {
            if (IsMineLocationInstance(location))
                return true;

            if (KnownIds.Contains(locationId))
                return true;

            if (locationId.StartsWith(UndergroundMinePrefix, StringComparison.OrdinalIgnoreCase))
                return true;

            string? type = data?.CreateOnLoad?.Type;
            if (!string.IsNullOrEmpty(type))
            {
                if (type.EndsWith(".Mine", StringComparison.Ordinal)
                    || type.EndsWith(".MineShaft", StringComparison.Ordinal)
                    || type.Contains("VolcanoDungeon", StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsMine(GameLocation? location)
        {
            return IsMine(location?.NameOrUniqueName ?? string.Empty, location: location);
        }

        public static bool TryGetBlocklistKey(string locationId, GameLocation? location, out string blocklistKey)
        {
            if (locationId.StartsWith(UndergroundMinePrefix, StringComparison.OrdinalIgnoreCase)
                || (IsMine(locationId, location: location) && location is MineShaft))
            {
                blocklistKey = UndergroundMinePrefix;
                return true;
            }

            if (KnownIds.Contains(locationId))
            {
                blocklistKey = locationId;
                return true;
            }

            if (IsMineLocationInstance(location))
            {
                blocklistKey = location switch
                {
                    Mine => "Mine",
                    MineShaft => UndergroundMinePrefix,
                    _ when location?.GetType().Name.Contains("VolcanoDungeon", StringComparison.Ordinal) == true
                        => "VolcanoDungeon",
                    _ => location!.NameOrUniqueName
                };
                return true;
            }

            blocklistKey = locationId;
            return false;
        }

        private static bool IsMineLocationInstance(GameLocation? location)
        {
            if (location is MineShaft or Mine)
                return true;

            string? typeName = location?.GetType().FullName;
            return typeName != null && typeName.Contains("VolcanoDungeon", StringComparison.Ordinal);
        }
    }

}
