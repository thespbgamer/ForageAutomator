using System.Collections.Generic;
using StardewValley;

namespace ForageAutomator
{
    public sealed class AreaCollectConfig
    {
        public Dictionary<string, ScopeRule> Blocked { get; set; } = new();

        public ScopeRule GetBlockRule(string locationId, GameLocation? location = null)
        {
            if (Blocked.TryGetValue(locationId, out ScopeRule? rule))
                return rule;

            if (MineLocationIds.TryGetBlocklistKey(locationId, location, out string blocklistKey)
                && !string.Equals(blocklistKey, locationId, System.StringComparison.OrdinalIgnoreCase)
                && Blocked.TryGetValue(blocklistKey, out rule))
            {
                return rule;
            }

            bool defaultMine = MineLocationIds.IsMine(locationId, location: location);
            return new ScopeRule
            {
                Auto = defaultMine,
                Manual = false
            };
        }

        public bool IsAutoBlocked(string locationId, GameLocation? location = null)
            => GetBlockRule(locationId, location).Auto;

        public bool IsManualBlocked(string locationId, GameLocation? location = null)
            => GetBlockRule(locationId, location).Manual;

        public void SetAutoBlocked(string locationId, bool blocked)
        {
            EnsureBlocked(locationId).Auto = blocked;
            PruneIfEmpty(locationId);
        }

        public void SetManualBlocked(string locationId, bool blocked)
        {
            EnsureBlocked(locationId).Manual = blocked;
            PruneIfEmpty(locationId);
        }

        private ScopeRule EnsureBlocked(string locationId)
        {
            if (!Blocked.TryGetValue(locationId, out ScopeRule? rule))
            {
                rule = new ScopeRule
                {
                    Auto = false,
                    Manual = false
                };
                Blocked[locationId] = rule;
            }

            return rule;
        }

        private void PruneIfEmpty(string locationId)
        {
            if (!Blocked.TryGetValue(locationId, out ScopeRule? rule) || rule.Auto || rule.Manual)
                return;

            if (MineLocationIds.IsMine(locationId))
                return;

            Blocked.Remove(locationId);
        }

        public void ResetToDefaults()
        {
            Blocked.Clear();
        }
    }

}
