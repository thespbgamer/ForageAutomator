using ForageAutomator.Automation;

namespace ForageAutomator
{
    public sealed class ItemCollectConfig
    {
        public ScopeRule GroundForage { get; set; } = new();

        public ScopeRule Bushes { get; set; } = new();

        public ScopeRule ArtifactSpots { get; set; } = new();

        public ScopeRule Panning { get; set; } = new();

        public ScopeRule GetRule(ForageType type)
        {
            return type switch
            {
                ForageType.Ground or ForageType.ForageCrop => GroundForage,
                ForageType.Bush => Bushes,
                ForageType.ArtifactSpot => ArtifactSpots,
                ForageType.Panning => Panning,
                _ => new ScopeRule()
            };
        }

        public void ResetToDefaults()
        {
            GroundForage = new ScopeRule();
            Bushes = new ScopeRule();
            ArtifactSpots = new ScopeRule();
            Panning = new ScopeRule();
        }
    }

}
