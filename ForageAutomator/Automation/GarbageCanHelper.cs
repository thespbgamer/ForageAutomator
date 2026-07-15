using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.GameData.Characters;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;

namespace ForageAutomator.Automation
{
    internal static class GarbageCanHelper
    {
        private const int WitnessRadius = 7;

        public static IEnumerable<GarbageCanInfo> GetGarbageCans(GameLocation location)
        {
            Layer? buildings = location.map?.GetLayer("Buildings");
            if (buildings == null)
                yield break;

            for (int x = 0; x < buildings.LayerWidth; x++)
            {
                for (int y = 0; y < buildings.LayerHeight; y++)
                {
                    Tile? tile = buildings.Tiles[x, y];
                    if (tile == null)
                        continue;

                    if (!tile.Properties.TryGetValue("Action", out PropertyValue? actionValue))
                        continue;

                    string? action = actionValue?.ToString();
                    if (action == null || !action.StartsWith("Garbage "))
                        continue;

                    string id = action["Garbage ".Length..].Trim();
                    if (string.IsNullOrWhiteSpace(id))
                        continue;

                    if (!CanCheckToday(id))
                        continue;

                    yield return new GarbageCanInfo
                    {
                        Id = id,
                        Tile = new Vector2(x, y)
                    };
                }
            }
        }

        public static bool CanCheckToday(string canId)
        {
            return !Game1.netWorldState.Value.CheckedGarbage.Contains(canId);
        }

        public static bool HasWitnessingNpc(GameLocation location, Farmer player)
        {
            Vector2 playerTile = player.Tile;

            foreach (NPC npc in location.characters)
            {
                if (!npc.IsVillager)
                    continue;

                if (!IsInWitnessRange(playerTile, npc.Tile))
                    continue;

                if (GetDumpsterDiveFriendshipEffect(npc) < 0)
                    return true;
            }

            return false;
        }

        public static bool ShouldBlockWitness(bool blockWhenWitnessed, GameLocation location, Farmer player)
        {
            return blockWhenWitnessed && HasWitnessingNpc(location, player);
        }

        public static bool CanSafelyLoot(GameLocation location, Farmer player, bool blockWhenWitnessed = true)
        {
            return !ShouldBlockWitness(blockWhenWitnessed, location, player);
        }

        public static bool TryLoot(
            GameLocation location,
            GarbageCanInfo can,
            Farmer player,
            bool blockWhenWitnessed = true,
            bool playAnimations = false)
        {
            if (!CanSafelyLoot(location, player, blockWhenWitnessed))
                return false;

            return location.CheckGarbage(
                can.Id,
                can.Tile,
                player,
                playAnimations,
                reactNpcs: true,
                logError: null);
        }

        private static bool IsInWitnessRange(Vector2 playerTile, Vector2 npcTile)
        {
            return System.Math.Abs(npcTile.X - playerTile.X) <= WitnessRadius
                && System.Math.Abs(npcTile.Y - playerTile.Y) <= WitnessRadius;
        }

        private static int GetDumpsterDiveFriendshipEffect(NPC npc)
        {
            CharacterData? data = npc.GetData();
            return data?.DumpsterDiveFriendshipEffect ?? -25;
        }
    }

}
