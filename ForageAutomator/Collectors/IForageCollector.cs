using StardewValley;

namespace ForageAutomator.Collectors
{
    internal interface IForageCollector
    {
        bool CanCollect(Automation.ForageTarget target);

        Automation.CollectResult TryCollect(GameLocation location, Farmer player, Automation.ForageTarget target);
    }

}
