using System;
using ForageAutomator.Automation;
using StardewModdingAPI;
using StardewValley;

namespace ForageAutomator.Notifications
{
    internal sealed class HudNotifier
    {
        private readonly ModConfig config;
        private readonly ITranslationHelper translation;
        private DateTime lastInventoryFullMessage = DateTime.MinValue;
        private DateTime lastMissingToolMessage = DateTime.MinValue;
        private DateTime lastRidingHorseMessage = DateTime.MinValue;
        private static readonly TimeSpan Cooldown = TimeSpan.FromSeconds(3);

        public HudNotifier(ModConfig config, ITranslationHelper translation)
        {
            this.config = config;
            this.translation = translation;
        }

        public void ShowInventoryFull()
        {
            if (!config.ShowHudMessages || !config.NotifyInventoryFull)
                return;

            if (DateTime.UtcNow - lastInventoryFullMessage < Cooldown)
                return;

            lastInventoryFullMessage = DateTime.UtcNow;
            ShowMessage(translation.Get("hud.inventory-full"), HUDMessage.error_type);
        }

        public void ShowMissingTool(RequiredToolKind tool)
        {
            if (!config.ShowHudMessages || !config.NotifyMissingTool)
                return;

            if (DateTime.UtcNow - lastMissingToolMessage < Cooldown)
                return;

            lastMissingToolMessage = DateTime.UtcNow;
            string toolName = ToolHelper.GetToolDisplayName(tool);
            ShowMessage(
                translation.Get("hud.missing-tool", new { tool = toolName }),
                HUDMessage.error_type);
        }

        public void ShowSweepComplete(int count, int experienceGained)
        {
            if (!config.ShowHudMessages)
                return;

            string message = config.ShowSweepExperience
                ? translation.Get("hud.sweep-complete-with-xp", new { count, xp = experienceGained })
                : translation.Get("hud.sweep-complete", new { count });

            ShowMessage(message, HUDMessage.achievement_type);
        }

        public void ShowSweepStarted()
        {
            if (!config.ShowHudMessages)
                return;

            ShowMessage(translation.Get("hud.sweep-started"), HUDMessage.newQuest_type);
        }

        public void ShowSweepCancelled()
        {
            if (!config.ShowHudMessages)
                return;

            ShowMessage(translation.Get("hud.sweep-cancelled"), HUDMessage.error_type);
        }

        public void ShowRidingHorseBlocked()
        {
            if (!config.ShowHudMessages || !config.NotifyRidingHorse)
                return;

            if (DateTime.UtcNow - lastRidingHorseMessage < Cooldown)
                return;

            lastRidingHorseMessage = DateTime.UtcNow;
            ShowMessage(translation.Get("hud.riding-horse"), HUDMessage.error_type);
        }

        private static void ShowMessage(string message, int iconType)
        {
            Game1.addHUDMessage(new HUDMessage(message, iconType));
        }
    }

}
