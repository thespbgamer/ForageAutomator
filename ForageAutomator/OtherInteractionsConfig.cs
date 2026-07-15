using ForageAutomator.Automation;
using ForageAutomator.Rendering;

namespace ForageAutomator
{
    public sealed class OtherInteractionsConfig
    {
        public InteractionRule CrabPots { get; set; } = InteractionRule.WithColor(ConfigColor.DefaultCrabPot);

        public InteractionRule FruitTrees { get; set; } = InteractionRule.WithColor(ConfigColor.DefaultFruitTree);

        public InteractionRule Machines { get; set; } = InteractionRule.WithColor(ConfigColor.DefaultMachine);

        public InteractionRule Tappers { get; set; } = InteractionRule.WithColor(ConfigColor.DefaultTapper);

        public InteractionRule BeeHouses { get; set; } = InteractionRule.WithColor(ConfigColor.DefaultBeeHouse);

        public InteractionRule MushroomBoxes { get; set; } = InteractionRule.WithColor(ConfigColor.DefaultMushroomBox);

        public InteractionRule GarbageCans { get; set; } = InteractionRule.WithColor(ConfigColor.DefaultGarbageCan, blockWhenWitnessed: true);

        public InteractionRule HayGrass { get; set; } = InteractionRule.WithColor(ConfigColor.DefaultHay);

        public InteractionRule GetRule(ForageType type)
        {
            return type switch
            {
                ForageType.CrabPot => CrabPots,
                ForageType.FruitTree => FruitTrees,
                ForageType.Machine => Machines,
                ForageType.Tapper => Tappers,
                ForageType.BeeHouse => BeeHouses,
                ForageType.MushroomBox => MushroomBoxes,
                ForageType.GarbageCan => GarbageCans,
                ForageType.HayGrass => HayGrass,
                _ => new InteractionRule()
            };
        }

        public void ResetToDefaults()
        {
            CrabPots = InteractionRule.WithColor(ConfigColor.DefaultCrabPot);
            FruitTrees = InteractionRule.WithColor(ConfigColor.DefaultFruitTree);
            Machines = InteractionRule.WithColor(ConfigColor.DefaultMachine);
            Tappers = InteractionRule.WithColor(ConfigColor.DefaultTapper);
            BeeHouses = InteractionRule.WithColor(ConfigColor.DefaultBeeHouse);
            MushroomBoxes = InteractionRule.WithColor(ConfigColor.DefaultMushroomBox);
            GarbageCans = InteractionRule.WithColor(ConfigColor.DefaultGarbageCan, blockWhenWitnessed: true);
            HayGrass = InteractionRule.WithColor(ConfigColor.DefaultHay);
        }
    }

}
