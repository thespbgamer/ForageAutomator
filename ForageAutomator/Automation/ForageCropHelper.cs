using StardewValley;
using StardewValley.TerrainFeatures;

namespace ForageAutomator.Automation
{
    internal static class ForageCropHelper
    {
        private const string SpringOnionCropId = "1";
        private const string GingerCropId = "2";

        public static bool IsHarvestable(HoeDirt dirt)
        {
            Crop? crop = dirt.crop;
            if (crop == null || !crop.forageCrop.Value || crop.dead.Value)
                return false;

            if (crop.whichForageCrop.Value == GingerCropId)
                return IsMatureForageCrop(crop);

            return dirt.readyForHarvest();
        }

        public static RequiredToolKind GetRequiredTool(Crop crop)
        {
            return crop.whichForageCrop.Value == GingerCropId
                ? RequiredToolKind.Hoe
                : RequiredToolKind.None;
        }

        public static string GetDisplayName(Crop crop)
        {
            return crop.whichForageCrop.Value switch
            {
                SpringOnionCropId => ItemRegistry.GetDataOrErrorItem("(O)399").DisplayName,
                GingerCropId => ItemRegistry.GetDataOrErrorItem("(O)829").DisplayName,
                _ => "forage"
            };
        }

        public static bool IsGinger(Crop crop) => crop.whichForageCrop.Value == GingerCropId;

        public static bool TryGetAt(GameLocation location, Microsoft.Xna.Framework.Vector2 tile, out HoeDirt dirt)
        {
            if (location.terrainFeatures.TryGetValue(tile, out TerrainFeature feature)
                && feature is HoeDirt hoeDirt
                && IsHarvestable(hoeDirt))
            {
                dirt = hoeDirt;
                return true;
            }

            dirt = null!;
            return false;
        }

        private static bool IsMatureForageCrop(Crop crop)
        {
            return crop.currentPhase.Value >= crop.phaseDays.Count - 1
                && (!crop.fullyGrown.Value || crop.dayOfCurrentPhase.Value <= 0);
        }
    }

}
