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
            scanCache = new ForageScanCache(config);
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
            LocationCatalog.Initialize(Helper);

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

            ConfigMenuBuilder.Register(api, Helper, config, ModManifest);
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
                or nameof(ModConfig.ItemRules)
                or nameof(ModConfig.OtherInteractions)
                or nameof(ModConfig.Areas)
                || fieldName.StartsWith("ItemRules.")
                || fieldName.StartsWith("OtherInteractions.")
                || fieldName.StartsWith("Areas.")
                || fieldName.StartsWith("Blocked.");
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
