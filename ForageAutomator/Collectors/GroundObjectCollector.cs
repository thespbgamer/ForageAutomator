using System;
using ForageAutomator.Automation;
using Microsoft.Xna.Framework;
using StardewValley;
using SObject = StardewValley.Object;

namespace ForageAutomator.Collectors
{
    internal sealed class GroundObjectCollector : IForageCollector
    {
        public bool CanCollect(ForageTarget target) => target.Type == ForageType.Ground;

        public CollectResult TryCollect(GameLocation location, Farmer player, ForageTarget target)
        {
            if (!ToolHelper.HasInventorySpace(player))
                return CollectResult.InventoryFull;

            if (target.Source is not SObject obj)
                return CollectResult.Failed;

            if (!location.objects.ContainsKey(target.Tile))
                return CollectResult.Skipped;

            if (ForageTargetScanner.IsArtifactSpotObject(obj))
                return CollectResult.Skipped;

            if (!CollectionHelper.CanInteractWith(player, target.Tile))
                return CollectResult.Failed;

            Random random = Utility.CreateDaySaveRandom(target.Tile.X, target.Tile.Y * 777f);
            int xp = 3;
            int skill = obj.isAnimalProduct() ? Farmer.farmingSkill : Farmer.foragingSkill;

            Item pickup = obj.getOne();

            if (player.professions.Contains(Farmer.botanist) && pickup is SObject pickupObj && pickupObj.isForage())
                pickupObj.Quality = 4;
            else if (pickup is SObject forageObj && forageObj.isForage())
                forageObj.Quality = DetermineForageQuality(player, random);

            player.gainExperience(skill, xp);

            if (player.addItemToInventory(pickup) != null)
                return CollectResult.InventoryFull;

            if (pickup is SObject spawned && spawned.isForage() && player.professions.Contains(Farmer.gatherer) && random.NextDouble() < 0.2)
            {
                Item bonus = obj.getOne();
                if (bonus is SObject bonusObj && bonusObj.isForage())
                    bonusObj.Quality = spawned.Quality;

                player.gainExperience(Farmer.foragingSkill, xp);
                if (player.addItemToInventory(bonus) != null)
                    Game1.createItemDebris(bonus, CollectionHelper.GetTileCenter(target.Tile), -1, location, -1);
            }

            if (location.removeObject(target.Tile, false) == null)
                location.objects.Remove(target.Tile);

            Game1.playSound("harvest");
            return CollectResult.Success;
        }

        private static int DetermineForageQuality(Farmer player, Random random)
        {
            float level = player.ForagingLevel;

            if (random.NextDouble() < level / 30.0)
                return 2;

            if (random.NextDouble() < level / 15.0)
                return 1;

            return 0;
        }
    }

}
