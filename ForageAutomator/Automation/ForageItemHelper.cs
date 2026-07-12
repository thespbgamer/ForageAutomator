using System;
using Microsoft.Xna.Framework;
using StardewValley;
using SObject = StardewValley.Object;

namespace ForageAutomator.Automation
{
    internal static class ForageItemHelper
    {
        public static void ApplyForageQuality(Farmer player, Item item, Random random)
        {
            if (item is not SObject forageObj || !forageObj.isForage())
                return;

            if (player.professions.Contains(Farmer.botanist))
            {
                forageObj.Quality = 4;
                return;
            }

            float level = player.ForagingLevel;

            if (random.NextDouble() < level / 30.0)
                forageObj.Quality = 2;
            else if (random.NextDouble() < level / 15.0)
                forageObj.Quality = 1;
        }

        public static void TryGathererBonus(Farmer player, Item sourceItem, Random random, Vector2 tile, GameLocation location)
        {
            if (!player.professions.Contains(Farmer.gatherer))
                return;

            if (sourceItem is not SObject forageObj || !forageObj.isForage())
                return;

            if (random.NextDouble() >= 0.2)
                return;

            Item bonus = sourceItem.getOne();
            if (bonus is SObject bonusObj)
                bonusObj.Quality = forageObj.Quality;

            player.gainExperience(Farmer.foragingSkill, 3);
            if (player.addItemToInventory(bonus) != null)
                Game1.createItemDebris(bonus, CollectionHelper.GetTileCenter(tile), -1, location, -1);
        }

        public static bool DropsOnGround(ForageType type)
        {
            return type is ForageType.Bush or ForageType.ArtifactSpot;
        }

        public static bool DropsOnGround(ForageTarget target)
        {
            return DropsOnGround(target.Type)
                || (target.Type == ForageType.ForageCrop && target.RequiredTool == RequiredToolKind.Hoe);
        }
    }

}
