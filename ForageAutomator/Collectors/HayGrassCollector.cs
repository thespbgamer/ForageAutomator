using ForageAutomator.Automation;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;

namespace ForageAutomator.Collectors
{
    internal sealed class HayGrassCollector : IForageCollector
    {
        public bool CanCollect(ForageTarget target) => target.Type == ForageType.HayGrass;

        public CollectResult TryCollect(GameLocation location, Farmer player, ForageTarget target)
        {
            if (!ToolHelper.HasScythe(player))
                return CollectResult.MissingTool;

            if (target.Source is not Grass grass || !HayGrassHelper.IsHarvestable(location, target.Tile, grass))
                return CollectResult.Skipped;

            if (!HayGrassHelper.HasHayStorage(location))
                return CollectResult.Skipped;

            if (!CollectionHelper.CanInteractWith(player, target.Tile))
                return CollectResult.Failed;

            MeleeWeapon? scythe = ToolHelper.FindScythe(player);
            if (scythe == null)
                return CollectResult.MissingTool;

            int previousSlot = ToolHelper.SelectTool(player, scythe);
            try
            {
                CollectionHelper.PreparePlayer(player, target.Tile);
                ToolHelper.ExecuteToolFunctionAt(location, player, scythe, target.Tile);
                return location.terrainFeatures.ContainsKey(target.Tile)
                    ? CollectResult.Failed
                    : CollectResult.Success;
            }
            finally
            {
                ToolHelper.RestoreToolSlot(player, previousSlot);
            }
        }
    }

}
