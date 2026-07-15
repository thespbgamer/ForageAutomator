namespace ForageAutomator
{
    public sealed class InteractionRule
    {
        /// <summary>Skip garbage cans when a villager would witness dumpster diving.</summary>
        public bool BlockWhenWitnessed { get; set; } = true;

        public bool Auto { get; set; } = false;

        public bool Manual { get; set; } = false;

        public bool ShowLines { get; set; } = false;

        public string LineColor { get; set; } = Rendering.ConfigColor.DefaultCrabPot;

        public static InteractionRule WithColor(string defaultColor, bool blockWhenWitnessed = false)
        {
            return new InteractionRule
            {
                LineColor = defaultColor,
                BlockWhenWitnessed = blockWhenWitnessed
            };
        }
    }

}
