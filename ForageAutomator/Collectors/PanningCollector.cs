using ForageAutomator.Automation;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Tools;

namespace ForageAutomator.Collectors
{
    internal sealed class PanningCollector : IForageCollector
    {
        public bool CanCollect(ForageTarget target) => target.Type == ForageType.Panning;

        public CollectResult TryCollect(GameLocation location, Farmer player, ForageTarget target)
        {
            if (!ToolHelper.HasCopperPan(player))
                return CollectResult.MissingTool;

            if (!ToolHelper.HasInventorySpace(player))
                return CollectResult.InventoryFull;

            if (!PanningHelper.IsActivePanTile(location, target.Tile))
                return CollectResult.Skipped;

            if (!PanningHelper.TryMoveToPanStand(location, player, target))
                return CollectResult.Failed;

            Pan? pan = ToolHelper.FindCopperPan(player);
            if (pan == null)
                return CollectResult.MissingTool;

            PanningHelper.PrepareForPanning(player, target.Tile);

            int previousSlot = ToolHelper.SelectTool(player, pan);
            try
            {
                return PanningHelper.TryExecutePan(location, player, pan, target.Tile)
                    ? CollectResult.Success
                    : CollectResult.Failed;
            }
            finally
            {
                ToolHelper.RestoreToolSlot(player, previousSlot);
                PanningHelper.ClearPanAnimationState(player);
            }
        }
    }

}
