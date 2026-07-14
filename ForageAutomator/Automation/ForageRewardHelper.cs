using System;
using Microsoft.Xna.Framework;
using StardewValley;
using SObject = StardewValley.Object;

namespace ForageAutomator.Automation
{
    internal static class ForageRewardHelper
    {
        public const int GroundForageXp = 7;
        public const int GathererBonusXp = 7;
        public const int SpringOnionXp = 3;
        public const int GingerXp = 7;
        public const int PanningForagingXp = 7;

        private static readonly int[] PanOreIds = { 378, 380, 384, 386 };

        public static void GrantGroundPickup(
            Farmer player,
            SObject source,
            Item pickup,
            Random random,
            Vector2 tile,
            GameLocation location)
        {
            int skill = source.isAnimalProduct() ? Farmer.farmingSkill : Farmer.foragingSkill;
            int xp = source.isForage() ? GroundForageXp : 3;

            if (pickup is SObject pickupObj && pickupObj.isForage())
                Game1.stats.ItemsForaged += (uint)pickup.Stack;

            player.gainExperience(skill, xp);

            if (pickup is not SObject spawned || !spawned.isForage() || !player.professions.Contains(Farmer.gatherer))
                return;

            if (random.NextDouble() >= 0.2)
                return;

            Item bonus = source.getOne();
            if (bonus is SObject bonusObj && bonusObj.isForage())
                bonusObj.Quality = spawned.Quality;

            player.gainExperience(Farmer.foragingSkill, GathererBonusXp);
            if (player.addItemToInventory(bonus) != null)
                Game1.createItemDebris(bonus, CollectionHelper.GetTileCenter(tile), -1, location, -1);
        }

        public static void GrantGingerHarvest(Farmer player, int itemsCollected = 1)
        {
            player.gainExperience(Farmer.foragingSkill, GingerXp);
            Game1.stats.ItemsForaged += (uint)Math.Max(1, itemsCollected);
        }

        public static void GrantPanning(Farmer player, System.Collections.Generic.IEnumerable<Item> items)
        {
            player.gainExperience(Farmer.foragingSkill, PanningForagingXp);

            foreach (Item? item in items)
            {
                if (item is not SObject obj || !IsPanOre(obj.ParentSheetIndex))
                    continue;

                player.gainExperience(Farmer.miningSkill, obj.Stack);
            }
        }

        private static bool IsPanOre(int parentSheetIndex)
        {
            foreach (int oreId in PanOreIds)
            {
                if (oreId == parentSheetIndex)
                    return true;
            }

            return false;
        }
    }

}
