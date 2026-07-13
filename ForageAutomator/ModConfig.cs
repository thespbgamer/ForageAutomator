using ForageAutomator.Rendering;
using StardewModdingAPI.Utilities;

namespace ForageAutomator
{
    public sealed class ModConfig
    {
        /// <summary>Auto-collect all forageables within pickup radius while walking.</summary>
        public bool AutoCollectOnRange { get; set; } = false;

        public int PickupRadius { get; set; } = 2;

        /// <summary>Automatically sweep the map when entering a location.</summary>
        public bool AutoCollectWholeMap { get; set; } = false;

        /// <summary>Walk to targets via pathfinding. When off, snap to each target instantly.</summary>
        public bool UsePathfinding { get; set; } = true;

        /// <summary>After a sweep finishes, return the player to where they stood when it started.</summary>
        public bool ReturnToStartAfterSweep { get; set; } = false;

        public KeybindList RangeKey { get; set; } = KeybindList.Parse("F5");

        public KeybindList WholeMapKey { get; set; } = KeybindList.Parse("F6");

        /// <summary>Include XP gained in the sweep-complete HUD message.</summary>
        public bool ShowSweepExperience { get; set; } = true;

        public bool CollectGroundForage
        {
            get => ItemRules.GroundForage.Manual;
            set => ItemRules.GroundForage.Manual = value;
        }

        public bool CollectBushes
        {
            get => ItemRules.Bushes.Manual;
            set => ItemRules.Bushes.Manual = value;
        }

        public bool CollectArtifactSpots
        {
            get => ItemRules.ArtifactSpots.Manual;
            set => ItemRules.ArtifactSpots.Manual = value;
        }

        public bool CollectPanning
        {
            get => ItemRules.Panning.Manual;
            set => ItemRules.Panning.Manual = value;
        }

        public ItemCollectConfig ItemRules { get; set; } = new();

        public AreaCollectConfig Areas { get; set; } = new();

        public bool ShowTargetLines { get; set; } = true;

        /// <summary>Max tile distance for target lines. 0 = entire visible map.</summary>
        public int LineRange { get; set; } = 0;

        public bool ShowLinesGroundForage { get; set; } = true;

        public bool ShowLinesBushes { get; set; } = true;

        public bool ShowLinesArtifactSpots { get; set; } = true;

        public bool ShowLinesEmptyBushes { get; set; } = false;

        public bool ShowLinesPanning { get; set; } = true;

        /// <summary>RGBA values separated by commas, e.g. 80,255,100,200.</summary>
        public string ColorLineReady { get; set; } = ConfigColor.DefaultReady;

        public string ColorLineOutOfRange { get; set; } = ConfigColor.DefaultOutOfRange;

        public string ColorLineMissingTool { get; set; } = ConfigColor.DefaultMissingTool;

        public string ColorLineInventoryFull { get; set; } = ConfigColor.DefaultInventoryFull;

        public string ColorLineUnreachable { get; set; } = ConfigColor.DefaultUnreachable;

        public string ColorLineEmptyBush { get; set; } = ConfigColor.DefaultEmptyBush;

        public bool ShowHudMessages { get; set; } = true;

        public bool NotifyInventoryFull { get; set; } = true;

        public bool NotifyMissingTool { get; set; } = true;

        public bool NotifyRidingHorse { get; set; } = true;

        public bool ShowSweepStartedMessage { get; set; } = true;

        public bool ShowSweepCancelledMessage { get; set; } = true;

        // Legacy config keys (SMAPI will populate if present in config.json).
        public bool EnablePassivePickup
        {
            get => AutoCollectOnRange;
            set => AutoCollectOnRange = value;
        }

        public bool EnableHotkeyWholeMap
        {
            get => true;
            set { }
        }

        public bool EnableHotkeySweep
        {
            get => true;
            set { }
        }

        public KeybindList ToggleKey
        {
            get => WholeMapKey;
            set => WholeMapKey = value;
        }

        public void ResetToDefaults()
        {
            ModConfig defaults = new();
            AutoCollectOnRange = defaults.AutoCollectOnRange;
            PickupRadius = defaults.PickupRadius;
            AutoCollectWholeMap = defaults.AutoCollectWholeMap;
            UsePathfinding = defaults.UsePathfinding;
            ReturnToStartAfterSweep = defaults.ReturnToStartAfterSweep;
            RangeKey = defaults.RangeKey;
            WholeMapKey = defaults.WholeMapKey;
            ShowSweepExperience = defaults.ShowSweepExperience;
            CollectGroundForage = defaults.CollectGroundForage;
            CollectBushes = defaults.CollectBushes;
            CollectArtifactSpots = defaults.CollectArtifactSpots;
            CollectPanning = defaults.CollectPanning;
            ItemRules.ResetToDefaults();
            Areas.ResetToDefaults();
            ShowTargetLines = defaults.ShowTargetLines;
            LineRange = defaults.LineRange;
            ShowLinesGroundForage = defaults.ShowLinesGroundForage;
            ShowLinesBushes = defaults.ShowLinesBushes;
            ShowLinesArtifactSpots = defaults.ShowLinesArtifactSpots;
            ShowLinesEmptyBushes = defaults.ShowLinesEmptyBushes;
            ShowLinesPanning = defaults.ShowLinesPanning;
            ColorLineReady = defaults.ColorLineReady;
            ColorLineOutOfRange = defaults.ColorLineOutOfRange;
            ColorLineMissingTool = defaults.ColorLineMissingTool;
            ColorLineInventoryFull = defaults.ColorLineInventoryFull;
            ColorLineUnreachable = defaults.ColorLineUnreachable;
            ColorLineEmptyBush = defaults.ColorLineEmptyBush;
            ShowHudMessages = defaults.ShowHudMessages;
            NotifyInventoryFull = defaults.NotifyInventoryFull;
            NotifyMissingTool = defaults.NotifyMissingTool;
            NotifyRidingHorse = defaults.NotifyRidingHorse;
            ShowSweepStartedMessage = defaults.ShowSweepStartedMessage;
            ShowSweepCancelledMessage = defaults.ShowSweepCancelledMessage;
        }
    }

}
