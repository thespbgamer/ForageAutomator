using System.Collections.Generic;
using System.Linq;
using ForageAutomator.Automation;
using StardewModdingAPI;
using StardewValley;

namespace ForageAutomator
{
    internal static class ForageTargetFilters
    {
        public static bool IsEnabledForCollect(ModConfig config, ForageType type, CollectScope scope, GameLocation location)
        {
            return CollectPolicy.ShouldCollect(config, location, type, scope);
        }

        public static bool IsEnabledForCollect(ModConfig config, ForageType type)
        {
            if (!Context.IsWorldReady)
                return false;

            return IsEnabledForCollect(config, type, CollectScope.Manual, Game1.currentLocation);
        }

        public static bool IsEnabledForLines(ModConfig config, ForageType type)
        {
            return type switch
            {
                ForageType.Ground or ForageType.ForageCrop => config.ShowLinesGroundForage,
                ForageType.Bush => config.ShowLinesBushes,
                ForageType.ArtifactSpot => config.ShowLinesArtifactSpots,
                ForageType.Panning => config.ShowLinesPanning,
                _ => true
            };
        }

        public static IEnumerable<ForageTarget> FilterForCollect(
            ModConfig config,
            IEnumerable<ForageTarget> targets,
            CollectScope scope,
            GameLocation location)
        {
            return targets.Where(t => IsEnabledForCollect(config, t.Type, scope, location));
        }

        public static IEnumerable<ForageTarget> FilterForLines(ModConfig config, IEnumerable<ForageTarget> targets)
        {
            return targets.Where(t => IsEnabledForLines(config, t.Type));
        }

        public static int? GetLineScanRadius(ModConfig config)
        {
            return config.LineRange <= 0 ? null : config.LineRange;
        }
    }

}
