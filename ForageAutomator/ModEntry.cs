using System;
using System.Linq;
using ForageAutomator.Automation;
using ForageAutomator.Notifications;
using ForageAutomator.Rendering;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace ForageAutomator
{
    public sealed class ModEntry : Mod
    {
        private ModConfig config = null!;
        private HudNotifier notifier = null!;
        private ForageScanCache scanCache = null!;
        private PassivePickupController passiveController = null!;
        private ForageRunController runController = null!;
        private ForageLineRenderer lineRenderer = null!;

        public override void Entry(IModHelper helper)
        {
            ModTranslations.Initialize(helper.Translation);
            config = helper.ReadConfig<ModConfig>();
            ConfigColor.Sanitize(config);

            notifier = new HudNotifier(config, helper.Translation);
            scanCache = new ForageScanCache();
            passiveController = new PassivePickupController(config, notifier, scanCache);
            runController = new ForageRunController(config, notifier, scanCache);
            lineRenderer = new ForageLineRenderer(config, passiveController, runController, scanCache);

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.GameLoop.TimeChanged += OnTimeChanged;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.Player.Warped += OnWarped;
            helper.Events.World.ObjectListChanged += OnObjectListChanged;
            helper.Events.Display.RenderedWorld += lineRenderer.OnRenderedWorld;

            helper.ConsoleCommands.Add("fa_start", Helper.Translation.Get("commands.fa.start.description"), StartSweep);
            helper.ConsoleCommands.Add("fa_stop", Helper.Translation.Get("commands.fa.stop.description"), StopSweep);
            helper.ConsoleCommands.Add("fa_status", Helper.Translation.Get("commands.fa.status.description"), ShowStatus);
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            var api = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (api == null)
                return;

            api.Register(
                mod: ModManifest,
                reset: () =>
                {
                    config.ResetToDefaults();
                    OnConfigChanged(null);
                },
                save: () =>
                {
                    OnConfigChanged(null);
                    Helper.WriteConfig(config);
                });

            api.OnFieldChanged(ModManifest, (fieldName, _) => OnConfigChanged(fieldName));

            api.AddSectionTitle(
                mod: ModManifest,
                text: () => Helper.Translation.Get("config.section.range"));

            api.AddBoolOption(
                mod: ModManifest,
                getValue: () => config.AutoCollectOnRange,
                setValue: value => config.AutoCollectOnRange = value,
                name: () => Helper.Translation.Get("config.auto-collect-on-range.name"),
                tooltip: () => Helper.Translation.Get("config.auto-collect-on-range.tooltip"));

            api.AddNumberOption(
                mod: ModManifest,
                getValue: () => config.PickupRadius,
                setValue: value => config.PickupRadius = value,
                name: () => Helper.Translation.Get("config.pickup-radius.name"),
                tooltip: () => Helper.Translation.Get("config.pickup-radius.tooltip"),
                min: 1,
                max: 10);

            api.AddBoolOption(
                mod: ModManifest,
                getValue: () => config.AutoCollectWholeMap,
                setValue: value => config.AutoCollectWholeMap = value,
                name: () => Helper.Translation.Get("config.auto-collect-whole-map.name"),
                tooltip: () => Helper.Translation.Get("config.auto-collect-whole-map.tooltip"));

            api.AddKeybindList(
                mod: ModManifest,
                getValue: () => config.RangeKey,
                setValue: value => config.RangeKey = value,
                name: () => Helper.Translation.Get("config.range-key.name"),
                tooltip: () => Helper.Translation.Get("config.range-key.tooltip"));

            api.AddKeybindList(
                mod: ModManifest,
                getValue: () => config.WholeMapKey,
                setValue: value => config.WholeMapKey = value,
                name: () => Helper.Translation.Get("config.whole-map-key.name"),
                tooltip: () => Helper.Translation.Get("config.whole-map-key.tooltip"));

            api.AddSectionTitle(
                mod: ModManifest,
                text: () => Helper.Translation.Get("config.section.collect-types"));

            api.AddBoolOption(
                mod: ModManifest,
                getValue: () => config.CollectGroundForage,
                setValue: value => config.CollectGroundForage = value,
                name: () => Helper.Translation.Get("config.collect-ground-forage.name"),
                tooltip: () => Helper.Translation.Get("config.collect-ground-forage.tooltip"));

            api.AddBoolOption(
                mod: ModManifest,
                getValue: () => config.CollectBushes,
                setValue: value => config.CollectBushes = value,
                name: () => Helper.Translation.Get("config.collect-bushes.name"),
                tooltip: () => Helper.Translation.Get("config.collect-bushes.tooltip"));

            api.AddBoolOption(
                mod: ModManifest,
                getValue: () => config.CollectArtifactSpots,
                setValue: value => config.CollectArtifactSpots = value,
                name: () => Helper.Translation.Get("config.collect-artifact-spots.name"),
                tooltip: () => Helper.Translation.Get("config.collect-artifact-spots.tooltip"));

            api.AddBoolOption(
                mod: ModManifest,
                getValue: () => config.CollectPanning,
                setValue: value => config.CollectPanning = value,
                name: () => Helper.Translation.Get("config.collect-panning.name"),
                tooltip: () => Helper.Translation.Get("config.collect-panning.tooltip"));

            api.AddSectionTitle(
                mod: ModManifest,
                text: () => Helper.Translation.Get("config.section.pathfinding-and-lines"));

            api.AddBoolOption(
                mod: ModManifest,
                getValue: () => config.UsePathfinding,
                setValue: value => config.UsePathfinding = value,
                name: () => Helper.Translation.Get("config.use-pathfinding.name"),
                tooltip: () => Helper.Translation.Get("config.use-pathfinding.tooltip"));

            api.AddBoolOption(
                mod: ModManifest,
                getValue: () => config.ReturnToStartAfterSweep,
                setValue: value => config.ReturnToStartAfterSweep = value,
                name: () => Helper.Translation.Get("config.return-to-start-after-sweep.name"),
                tooltip: () => Helper.Translation.Get("config.return-to-start-after-sweep.tooltip"));

            api.AddBoolOption(
                mod: ModManifest,
                getValue: () => config.ShowTargetLines,
                setValue: value => config.ShowTargetLines = value,
                name: () => Helper.Translation.Get("config.show-target-lines.name"),
                tooltip: () => Helper.Translation.Get("config.show-target-lines.tooltip"));

            api.AddNumberOption(
                mod: ModManifest,
                getValue: () => config.LineRange,
                setValue: value => config.LineRange = value,
                name: () => Helper.Translation.Get("config.line-range.name"),
                tooltip: () => Helper.Translation.Get("config.line-range.tooltip"),
                min: 0,
                max: 200,
                formatValue: value => value == 0
                    ? Helper.Translation.Get("config.line-range.infinite")
                    : value.ToString());

            api.AddSectionTitle(
                mod: ModManifest,
                text: () => Helper.Translation.Get("config.section.line-types"));

            api.AddBoolOption(
                mod: ModManifest,
                getValue: () => config.ShowLinesGroundForage,
                setValue: value => config.ShowLinesGroundForage = value,
                name: () => Helper.Translation.Get("config.show-lines-ground-forage.name"),
                tooltip: () => Helper.Translation.Get("config.show-lines-ground-forage.tooltip"));

            api.AddBoolOption(
                mod: ModManifest,
                getValue: () => config.ShowLinesBushes,
                setValue: value => config.ShowLinesBushes = value,
                name: () => Helper.Translation.Get("config.show-lines-bushes.name"),
                tooltip: () => Helper.Translation.Get("config.show-lines-bushes.tooltip"));

            api.AddBoolOption(
                mod: ModManifest,
                getValue: () => config.ShowLinesEmptyBushes,
                setValue: value => config.ShowLinesEmptyBushes = value,
                name: () => Helper.Translation.Get("config.show-lines-empty-bushes.name"),
                tooltip: () => Helper.Translation.Get("config.show-lines-empty-bushes.tooltip"));

            api.AddBoolOption(
                mod: ModManifest,
                getValue: () => config.ShowLinesArtifactSpots,
                setValue: value => config.ShowLinesArtifactSpots = value,
                name: () => Helper.Translation.Get("config.show-lines-artifact-spots.name"),
                tooltip: () => Helper.Translation.Get("config.show-lines-artifact-spots.tooltip"));

            api.AddBoolOption(
                mod: ModManifest,
                getValue: () => config.ShowLinesPanning,
                setValue: value => config.ShowLinesPanning = value,
                name: () => Helper.Translation.Get("config.show-lines-panning.name"),
                tooltip: () => Helper.Translation.Get("config.show-lines-panning.tooltip"));

            api.AddSectionTitle(
                mod: ModManifest,
                text: () => Helper.Translation.Get("config.section.line-colors"));

            RegisterLineColorOption(api, () => config.ColorLineReady, value => config.ColorLineReady = value,
                "config.color-line-ready.name", ConfigColor.DefaultReady);
            RegisterLineColorOption(api, () => config.ColorLineOutOfRange, value => config.ColorLineOutOfRange = value,
                "config.color-line-out-of-range.name", ConfigColor.DefaultOutOfRange);
            RegisterLineColorOption(api, () => config.ColorLineMissingTool, value => config.ColorLineMissingTool = value,
                "config.color-line-missing-tool.name", ConfigColor.DefaultMissingTool);
            RegisterLineColorOption(api, () => config.ColorLineInventoryFull, value => config.ColorLineInventoryFull = value,
                "config.color-line-inventory-full.name", ConfigColor.DefaultInventoryFull);
            RegisterLineColorOption(api, () => config.ColorLineUnreachable, value => config.ColorLineUnreachable = value,
                "config.color-line-unreachable.name", ConfigColor.DefaultUnreachable);
            RegisterLineColorOption(api, () => config.ColorLineEmptyBush, value => config.ColorLineEmptyBush = value,
                "config.color-line-empty-bush.name", ConfigColor.DefaultEmptyBush);

            api.AddSectionTitle(
                mod: ModManifest,
                text: () => Helper.Translation.Get("config.section.hud-messages"));

            api.AddBoolOption(
                mod: ModManifest,
                getValue: () => config.ShowHudMessages,
                setValue: value => config.ShowHudMessages = value,
                name: () => Helper.Translation.Get("config.show-hud-messages.name"),
                tooltip: () => Helper.Translation.Get("config.show-hud-messages.tooltip"));

            api.AddBoolOption(
                mod: ModManifest,
                getValue: () => config.NotifyInventoryFull,
                setValue: value => config.NotifyInventoryFull = value,
                name: () => Helper.Translation.Get("config.notify-inventory-full.name"),
                tooltip: () => Helper.Translation.Get("config.notify-inventory-full.tooltip"));

            api.AddBoolOption(
                mod: ModManifest,
                getValue: () => config.NotifyMissingTool,
                setValue: value => config.NotifyMissingTool = value,
                name: () => Helper.Translation.Get("config.notify-missing-tool.name"),
                tooltip: () => Helper.Translation.Get("config.notify-missing-tool.tooltip"));

            api.AddBoolOption(
                mod: ModManifest,
                getValue: () => config.NotifyRidingHorse,
                setValue: value => config.NotifyRidingHorse = value,
                name: () => Helper.Translation.Get("config.notify-riding-horse.name"),
                tooltip: () => Helper.Translation.Get("config.notify-riding-horse.tooltip"));

            api.AddBoolOption(
                mod: ModManifest,
                getValue: () => config.ShowSweepExperience,
                setValue: value => config.ShowSweepExperience = value,
                name: () => Helper.Translation.Get("config.show-sweep-experience.name"),
                tooltip: () => Helper.Translation.Get("config.show-sweep-experience.tooltip"));
        }

        private void OnConfigChanged(string? fieldName)
        {
            ConfigColor.Sanitize(config);
            lineRenderer.Invalidate();
            passiveController.ClearSkippedTargets();
            runController.ClearSkippedTargets();

            if (AffectsCollection(fieldName))
                runController.OnCollectSettingsChanged();
            else
                WalkabilityHelper.InvalidateReachabilityCache();

            scanCache.Invalidate();
        }

        private static bool AffectsCollection(string? fieldName)
        {
            if (fieldName == null)
                return true;

            return fieldName is nameof(ModConfig.AutoCollectOnRange)
                or nameof(ModConfig.AutoCollectWholeMap)
                or nameof(ModConfig.PickupRadius)
                or nameof(ModConfig.UsePathfinding)
                or nameof(ModConfig.CollectGroundForage)
                or nameof(ModConfig.CollectBushes)
                or nameof(ModConfig.CollectArtifactSpots)
                or nameof(ModConfig.CollectPanning)
                or nameof(ModConfig.EnablePassivePickup);
        }

        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            OnLocationContentChanged();
        }

        private void OnObjectListChanged(object? sender, ObjectListChangedEventArgs e)
        {
            if (!Context.IsWorldReady || e.Location != Game1.currentLocation)
                return;

            if (!e.Added.Any() && !e.Removed.Any())
                return;

            scanCache.Invalidate();
            lineRenderer.Invalidate();
            passiveController.ClearSkippedTargets();
        }

        private void OnLocationContentChanged()
        {
            scanCache.Invalidate();
            lineRenderer.Invalidate();
            passiveController.ClearSkippedTargets();
            runController.OnLocationContentChanged();
        }

        private void OnTimeChanged(object? sender, TimeChangedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            OnLocationContentChanged();
        }

        private void RegisterLineColorOption(
            IGenericModConfigMenuApi api,
            Func<string> getValue,
            Action<string> setValue,
            string nameKey,
            string defaultValue)
        {
            api.AddTextOption(
                mod: ModManifest,
                getValue: getValue,
                setValue: value => setValue(ConfigColor.SanitizeValue(value, defaultValue)),
                name: () => Helper.Translation.Get(nameKey),
                tooltip: () => Helper.Translation.Get("config.color-line.tooltip"));
        }

        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            passiveController.UpdateTicked(runController.IsRunning);
            lineRenderer.UpdateTicked();
            runController.UpdateTicked();
        }

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (config.RangeKey.JustPressed())
            {
                Helper.Input.Suppress(e.Button);
                runController.StartRange();
                return;
            }

            if (config.WholeMapKey.JustPressed())
            {
                Helper.Input.Suppress(e.Button);
                runController.StartWholeMap();
            }
        }

        private void OnWarped(object? sender, WarpedEventArgs e)
        {
            if (!e.IsLocalPlayer)
                return;

            runController.OnWarped();

            if (config.AutoCollectWholeMap)
                runController.ScheduleAutoStart();
        }

        private void StartSweep(string command, string[] args)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log(Helper.Translation.Get("commands.load-save-first"), LogLevel.Info);
                return;
            }

            runController.StartWholeMap();
        }

        private void StopSweep(string command, string[] args)
        {
            runController.Cancel();
        }

        private void ShowStatus(string command, string[] args)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log(Helper.Translation.Get("commands.load-save-first"), LogLevel.Info);
                return;
            }

            Monitor.Log(
                Helper.Translation.Get("commands.fa.status.output", new
                {
                    running = runController.IsRunning,
                    state = runController.State.ToString(),
                    queue = runController.QueueCount,
                    skipped = runController.SkippedTargets.Count + passiveController.SkippedTargets.Count
                }),
                LogLevel.Info);
        }
    }

}
