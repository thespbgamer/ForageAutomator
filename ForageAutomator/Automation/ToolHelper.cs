using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Tools;
using SObject = StardewValley.Object;

namespace ForageAutomator.Automation
{
    internal static class ToolHelper
    {
        public static bool HasHoe(Farmer player)
        {
            return FindHoe(player) != null;
        }

        public static bool HasCopperPan(Farmer player)
        {
            return FindCopperPan(player) != null;
        }

        public static Hoe? FindHoe(Farmer player)
        {
            foreach (Item? item in player.Items)
            {
                if (item is Hoe hoe)
                    return hoe;
            }

            return null;
        }

        public static Pan? FindCopperPan(Farmer player)
        {
            foreach (Item? item in player.Items)
            {
                if (item is Pan pan)
                    return pan;
            }

            return null;
        }

        public static bool HasRequiredTool(Farmer player, RequiredToolKind tool)
        {
            return tool switch
            {
                RequiredToolKind.Hoe => HasHoe(player),
                RequiredToolKind.CopperPan => HasCopperPan(player),
                _ => true
            };
        }

        public static bool HasInventorySpace(Farmer player)
        {
            return !player.isInventoryFull();
        }

        public static int SelectTool(Farmer player, Item tool)
        {
            int previousSlot = player.CurrentToolIndex;

            for (int i = 0; i < player.Items.Count; i++)
            {
                if (player.Items[i] == tool)
                {
                    player.CurrentToolIndex = i;
                    return previousSlot;
                }
            }

            return previousSlot;
        }

        public static void RestoreToolSlot(Farmer player, int slot)
        {
            if (slot >= 0 && slot < player.Items.Count)
                player.CurrentToolIndex = slot;
        }

        public static void ExecuteToolFunctionAt(GameLocation location, Farmer player, Tool tool, Vector2 targetTile)
        {
            tool.lastUser = player;
            player.toolPower.Value = 0;
            player.toolHold.Value = 0;

            Vector2 click = CollectionHelper.GetTileCenter(targetTile);
            int x = (int)click.X;
            int y = (int)click.Y;
            player.lastClick = click;

            float staminaBefore = player.Stamina;
            tool.DoFunction(location, x, y, 1, player);
            player.checkForExhaustion(staminaBefore);
            ToolAnimationHelper.Cancel(player);
        }

        public static void ExecuteToolFunction(GameLocation location, Farmer player, Tool tool)
        {
            ExecuteToolFunctionAt(location, player, tool, player.GetToolLocation(false) / Game1.tileSize);
        }

        public static bool UseHoeOnTile(GameLocation location, Farmer player, Vector2 targetTile)
        {
            Hoe? hoe = FindHoe(player);
            if (hoe == null)
                return false;

            int previousSlot = SelectTool(player, hoe);
            try
            {
                CollectionHelper.PreparePlayer(player, targetTile);
                ExecuteToolFunctionAt(location, player, hoe, targetTile);
                return true;
            }
            finally
            {
                RestoreToolSlot(player, previousSlot);
            }
        }

        public static bool UseHoeOnObject(GameLocation location, Farmer player, SObject obj, Vector2 tile)
        {
            Hoe? hoe = FindHoe(player);
            if (hoe == null)
                return false;

            int previousSlot = SelectTool(player, hoe);
            try
            {
                CollectionHelper.PreparePlayer(player, tile);
                hoe.lastUser = player;
                obj.performToolAction(hoe);
                ToolAnimationHelper.Cancel(player);
                return true;
            }
            finally
            {
                RestoreToolSlot(player, previousSlot);
            }
        }

        public static string GetToolDisplayName(RequiredToolKind tool)
        {
            return tool switch
            {
                RequiredToolKind.Hoe => "hoe",
                RequiredToolKind.CopperPan => "pan",
                _ => "tool"
            };
        }
    }

}
