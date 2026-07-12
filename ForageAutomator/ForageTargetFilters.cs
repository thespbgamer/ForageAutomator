using System.Collections.Generic;
using System.Linq;
using ForageAutomator.Automation;

namespace ForageAutomator
{
    internal static class ForageTargetFilters
    {
        public static bool IsEnabledForCollect(ModConfig config, ForageType type)
        {
            return type switch
            {
                ForageType.Ground or ForageType.ForageCrop => config.CollectGroundForage,
                ForageType.Bush => config.CollectBushes,
                ForageType.ArtifactSpot => config.CollectArtifactSpots,
                ForageType.Panning => config.CollectPanning,
                _ => true
            };
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

        public static IEnumerable<ForageTarget> FilterForCollect(ModConfig config, IEnumerable<ForageTarget> targets)
        {
            return targets.Where(t => IsEnabledForCollect(config, t.Type));
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
