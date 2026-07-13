using ForageAutomator.Automation;
using StardewValley;

namespace ForageAutomator
{
    internal static class CollectPolicy
    {
        public static bool IsLocationAllowed(ModConfig config, GameLocation? location, CollectScope scope)
        {
            if (location == null)
                return false;

            ScopeRule blocked = config.Areas.GetBlockRule(location.NameOrUniqueName, location);
            bool isBlocked = scope == CollectScope.Auto ? blocked.Auto : blocked.Manual;
            return !isBlocked;
        }

        public static bool IsLocationAllowed(ModConfig config, string locationId, CollectScope scope)
        {
            ScopeRule blocked = config.Areas.GetBlockRule(locationId);
            bool isBlocked = scope == CollectScope.Auto ? blocked.Auto : blocked.Manual;
            return !isBlocked;
        }

        public static bool IsTypeAllowed(ModConfig config, ForageType type, CollectScope scope)
        {
            ScopeRule rule = config.ItemRules.GetRule(type);
            return scope == CollectScope.Auto ? rule.Auto : rule.Manual;
        }

        public static bool ShouldCollect(ModConfig config, GameLocation location, ForageType type, CollectScope scope)
        {
            return IsLocationAllowed(config, location, scope)
                && IsTypeAllowed(config, type, scope);
        }
    }

}
