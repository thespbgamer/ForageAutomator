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

        public static string GetToolDisplayName(RequiredToolKind tool)
        {
            return tool switch
            {
                RequiredToolKind.Hoe => "hoe",
                RequiredToolKind.CopperPan => "copper pan",
                _ => "tool"
            };
        }
    }

}
