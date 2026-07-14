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
    }

}
