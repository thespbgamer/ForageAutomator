using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;

namespace ForageAutomator.Automation
{
    /// <summary>
    /// Caches forage scans and pathfinding. Refreshed on in-game time steps and map events, not every frame.
    /// </summary>
    internal sealed class ForageScanCache
    {
        private readonly ForageTargetScanner scanner = new();
        private List<ForageTarget> allTargets = new();
        private bool dirty = true;
        private bool justRefreshed;
        private string? locationName;

        public void Invalidate()
        {
            dirty = true;
        }

        public bool ConsumeJustRefreshed()
        {
            if (!justRefreshed)
                return false;

            justRefreshed = false;
            return true;
        }

        public IReadOnlyList<ForageTarget> GetAllTargets(GameLocation location, Farmer player)
        {
            EnsureFresh(location, player);
            return allTargets;
        }

        public IReadOnlyList<ForageTarget> GetTargetsInRadius(GameLocation location, Farmer player, int pickupRadius)
        {
            EnsureFresh(location, player);
            return allTargets
                .Where(t => CollectionHelper.IsWithinPickupRange(player, t.Tile, pickupRadius))
                .ToList();
        }

        public IReadOnlyList<ForageTarget> GetTargetsInTileRadius(GameLocation location, Farmer player, int tileRadius)
        {
            EnsureFresh(location, player);
            Vector2 playerTile = player.Tile;
            return allTargets
                .Where(t => WalkabilityHelper.IsWithinRadius(playerTile, t.Tile, tileRadius))
                .ToList();
        }

        public bool TryRefreshIfNeeded(GameLocation location, Farmer player)
        {
            if (!dirty)
                return false;

            EnsureFresh(location, player);
            return true;
        }

        private void EnsureFresh(GameLocation location, Farmer player)
        {
            string currentLocation = location.NameOrUniqueName;
            if (!dirty && locationName == currentLocation)
                return;

            allTargets = scanner.Scan(location, player).ToList();
            scanner.ApplyReachability(location, player, allTargets);
            locationName = currentLocation;
            dirty = false;
            justRefreshed = true;
        }
    }

}
