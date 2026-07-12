using Microsoft.Xna.Framework;

namespace ForageAutomator.Automation
{
    public enum ForageType
    {
        Ground,
        ForageCrop,
        Bush,
        ArtifactSpot,
        Panning
    }

    public enum RequiredToolKind
    {
        None,
        Hoe,
        CopperPan
    }

    public enum SkipReason
    {
        None,
        InventoryFull,
        MissingTool,
        Unreachable,
        OutOfRange,
        OnHorse,
        EmptyBush
    }

    public sealed class ForageTarget
    {
        public Vector2 Tile { get; init; }
        public Vector2 StandTile { get; set; }
        public ForageType Type { get; init; }
        public RequiredToolKind RequiredTool { get; init; }
        public object? Source { get; init; }
        public string DisplayName { get; init; } = "";
        public SkipReason SkipReason { get; set; } = SkipReason.None;
        public float DistanceFromPlayer { get; set; }

        public bool IsSkipped => SkipReason != SkipReason.None;
    }

    public enum CollectResult
    {
        Success,
        InventoryFull,
        MissingTool,
        Failed,
        Skipped
    }

}
