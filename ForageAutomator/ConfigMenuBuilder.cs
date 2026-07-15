using System;
using ForageAutomator.Automation;
using ForageAutomator.Rendering;
using StardewModdingAPI;

namespace ForageAutomator
{
    internal static class ConfigMenuBuilder
    {
        public static void Register(
            IGenericModConfigMenuApi api,
            IModHelper helper,
            ModConfig config,
            IManifest manifest)
        {
            ITranslationHelper translation = helper.Translation;

            api.AddPageLink(
                mod: manifest,
                pageId: "main",
                text: () => translation.Get("config.page.main.link"),
                tooltip: () => translation.Get("config.page.main.link.tooltip"));

            api.AddPageLink(
                mod: manifest,
                pageId: "items",
                text: () => translation.Get("config.page.items.link"),
                tooltip: () => translation.Get("config.page.items.link.tooltip"));

            api.AddPageLink(
                mod: manifest,
                pageId: "areas",
                text: () => translation.Get("config.page.areas.link"),
                tooltip: () => translation.Get("config.page.areas.link.tooltip"));

            api.AddPageLink(
                mod: manifest,
                pageId: "other-interactions",
                text: () => translation.Get("config.page.other-interactions.link"),
                tooltip: () => translation.Get("config.page.other-interactions.link.tooltip"));

            RegisterMainPage(api, helper, config, manifest);
            RegisterItemsPage(api, helper, config, manifest);
            RegisterAreasPage(api, helper, config, manifest);
            RegisterOtherInteractionsPage(api, helper, config, manifest);
        }

        private static void RegisterMainPage(
            IGenericModConfigMenuApi api,
            IModHelper helper,
            ModConfig config,
            IManifest manifest)
        {
            ITranslationHelper translation = helper.Translation;

            api.AddPage(manifest, "main", () => translation.Get("config.page.main.title"));

            api.AddSectionTitle(
                mod: manifest,
                text: () => translation.Get("config.section.range"));

            api.AddBoolOption(
                mod: manifest,
                getValue: () => config.AutoCollectOnRange,
                setValue: value => config.AutoCollectOnRange = value,
                name: () => translation.Get("config.auto-collect-on-range.name"),
                tooltip: () => translation.Get("config.auto-collect-on-range.tooltip"));

            api.AddNumberOption(
                mod: manifest,
                getValue: () => config.PickupRadius,
                setValue: value => config.PickupRadius = value,
                name: () => translation.Get("config.pickup-radius.name"),
                tooltip: () => translation.Get("config.pickup-radius.tooltip"),
                min: 1,
                max: 10);

            api.AddBoolOption(
                mod: manifest,
                getValue: () => config.AutoCollectWholeMap,
                setValue: value => config.AutoCollectWholeMap = value,
                name: () => translation.Get("config.auto-collect-whole-map.name"),
                tooltip: () => translation.Get("config.auto-collect-whole-map.tooltip"));

            api.AddKeybindList(
                mod: manifest,
                getValue: () => config.RangeKey,
                setValue: value => config.RangeKey = value,
                name: () => translation.Get("config.range-key.name"),
                tooltip: () => translation.Get("config.range-key.tooltip"));

            api.AddKeybindList(
                mod: manifest,
                getValue: () => config.WholeMapKey,
                setValue: value => config.WholeMapKey = value,
                name: () => translation.Get("config.whole-map-key.name"),
                tooltip: () => translation.Get("config.whole-map-key.tooltip"));

            api.AddSectionTitle(
                mod: manifest,
                text: () => translation.Get("config.section.pathfinding-and-lines"));

            api.AddBoolOption(
                mod: manifest,
                getValue: () => config.UsePathfinding,
                setValue: value => config.UsePathfinding = value,
                name: () => translation.Get("config.use-pathfinding.name"),
                tooltip: () => translation.Get("config.use-pathfinding.tooltip"));

            api.AddBoolOption(
                mod: manifest,
                getValue: () => config.ReturnToStartAfterSweep,
                setValue: value => config.ReturnToStartAfterSweep = value,
                name: () => translation.Get("config.return-to-start-after-sweep.name"),
                tooltip: () => translation.Get("config.return-to-start-after-sweep.tooltip"));

            api.AddBoolOption(
                mod: manifest,
                getValue: () => config.ShowTargetLines,
                setValue: value => config.ShowTargetLines = value,
                name: () => translation.Get("config.show-target-lines.name"),
                tooltip: () => translation.Get("config.show-target-lines.tooltip"));

            api.AddNumberOption(
                mod: manifest,
                getValue: () => config.LineRange,
                setValue: value => config.LineRange = value,
                name: () => translation.Get("config.line-range.name"),
                tooltip: () => translation.Get("config.line-range.tooltip"),
                min: 0,
                max: 200,
                formatValue: value => value == 0
                    ? translation.Get("config.line-range.infinite")
                    : value.ToString());

            api.AddSectionTitle(
                mod: manifest,
                text: () => translation.Get("config.section.line-types"));

            api.AddBoolOption(
                mod: manifest,
                getValue: () => config.ShowLinesGroundForage,
                setValue: value => config.ShowLinesGroundForage = value,
                name: () => translation.Get("config.show-lines-ground-forage.name"),
                tooltip: () => translation.Get("config.show-lines-ground-forage.tooltip"));

            api.AddBoolOption(
                mod: manifest,
                getValue: () => config.ShowLinesBushes,
                setValue: value => config.ShowLinesBushes = value,
                name: () => translation.Get("config.show-lines-bushes.name"),
                tooltip: () => translation.Get("config.show-lines-bushes.tooltip"));

            api.AddBoolOption(
                mod: manifest,
                getValue: () => config.ShowLinesEmptyBushes,
                setValue: value => config.ShowLinesEmptyBushes = value,
                name: () => translation.Get("config.show-lines-empty-bushes.name"),
                tooltip: () => translation.Get("config.show-lines-empty-bushes.tooltip"));

            api.AddBoolOption(
                mod: manifest,
                getValue: () => config.ShowLinesArtifactSpots,
                setValue: value => config.ShowLinesArtifactSpots = value,
                name: () => translation.Get("config.show-lines-artifact-spots.name"),
                tooltip: () => translation.Get("config.show-lines-artifact-spots.tooltip"));

            api.AddBoolOption(
                mod: manifest,
                getValue: () => config.ShowLinesPanning,
                setValue: value => config.ShowLinesPanning = value,
                name: () => translation.Get("config.show-lines-panning.name"),
                tooltip: () => translation.Get("config.show-lines-panning.tooltip"));

            api.AddSectionTitle(
                mod: manifest,
                text: () => translation.Get("config.section.line-colors"));

            RegisterLineColorOption(api, manifest, translation,
                () => config.ColorLineReady, value => config.ColorLineReady = value,
                "config.color-line-ready.name", ConfigColor.DefaultReady);
            RegisterLineColorOption(api, manifest, translation,
                () => config.ColorLineOutOfRange, value => config.ColorLineOutOfRange = value,
                "config.color-line-out-of-range.name", ConfigColor.DefaultOutOfRange);
            RegisterLineColorOption(api, manifest, translation,
                () => config.ColorLineMissingTool, value => config.ColorLineMissingTool = value,
                "config.color-line-missing-tool.name", ConfigColor.DefaultMissingTool);
            RegisterLineColorOption(api, manifest, translation,
                () => config.ColorLineInventoryFull, value => config.ColorLineInventoryFull = value,
                "config.color-line-inventory-full.name", ConfigColor.DefaultInventoryFull);
            RegisterLineColorOption(api, manifest, translation,
                () => config.ColorLineUnreachable, value => config.ColorLineUnreachable = value,
                "config.color-line-unreachable.name", ConfigColor.DefaultUnreachable);
            RegisterLineColorOption(api, manifest, translation,
                () => config.ColorLineEmptyBush, value => config.ColorLineEmptyBush = value,
                "config.color-line-empty-bush.name", ConfigColor.DefaultEmptyBush);

            api.AddSectionTitle(
                mod: manifest,
                text: () => translation.Get("config.section.hud-messages"));

            api.AddBoolOption(
                mod: manifest,
                getValue: () => config.ShowHudMessages,
                setValue: value => config.ShowHudMessages = value,
                name: () => translation.Get("config.show-hud-messages.name"),
                tooltip: () => translation.Get("config.show-hud-messages.tooltip"));

            api.AddBoolOption(
                mod: manifest,
                getValue: () => config.NotifyInventoryFull,
                setValue: value => config.NotifyInventoryFull = value,
                name: () => translation.Get("config.notify-inventory-full.name"),
                tooltip: () => translation.Get("config.notify-inventory-full.tooltip"));

            api.AddBoolOption(
                mod: manifest,
                getValue: () => config.NotifyMissingTool,
                setValue: value => config.NotifyMissingTool = value,
                name: () => translation.Get("config.notify-missing-tool.name"),
                tooltip: () => translation.Get("config.notify-missing-tool.tooltip"));

            api.AddBoolOption(
                mod: manifest,
                getValue: () => config.NotifyRidingHorse,
                setValue: value => config.NotifyRidingHorse = value,
                name: () => translation.Get("config.notify-riding-horse.name"),
                tooltip: () => translation.Get("config.notify-riding-horse.tooltip"));

            api.AddBoolOption(
                mod: manifest,
                getValue: () => config.ShowSweepExperience,
                setValue: value => config.ShowSweepExperience = value,
                name: () => translation.Get("config.show-sweep-experience.name"),
                tooltip: () => translation.Get("config.show-sweep-experience.tooltip"));

            api.AddBoolOption(
                mod: manifest,
                getValue: () => config.ShowSweepStartedMessage,
                setValue: value => config.ShowSweepStartedMessage = value,
                name: () => translation.Get("config.show-sweep-started-message.name"),
                tooltip: () => translation.Get("config.show-sweep-started-message.tooltip"));

            api.AddBoolOption(
                mod: manifest,
                getValue: () => config.ShowSweepCancelledMessage,
                setValue: value => config.ShowSweepCancelledMessage = value,
                name: () => translation.Get("config.show-sweep-cancelled-message.name"),
                tooltip: () => translation.Get("config.show-sweep-cancelled-message.tooltip"));
        }

        private static void RegisterOtherInteractionsPage(
            IGenericModConfigMenuApi api,
            IModHelper helper,
            ModConfig config,
            IManifest manifest)
        {
            ITranslationHelper translation = helper.Translation;

            api.AddPage(manifest, "other-interactions", () => translation.Get("config.page.other-interactions.title"));

            api.AddParagraph(manifest, () => translation.Get("config.page.other-interactions.description"));

            RegisterOtherInteractionOptions(api, manifest, translation, config, "crab-pots", () => config.OtherInteractions.CrabPots, ConfigColor.DefaultCrabPot);
            RegisterOtherInteractionOptions(api, manifest, translation, config, "fruit-trees", () => config.OtherInteractions.FruitTrees, ConfigColor.DefaultFruitTree);
            RegisterOtherInteractionOptions(api, manifest, translation, config, "machines", () => config.OtherInteractions.Machines, ConfigColor.DefaultMachine);
            RegisterOtherInteractionOptions(api, manifest, translation, config, "tappers", () => config.OtherInteractions.Tappers, ConfigColor.DefaultTapper);
            RegisterOtherInteractionOptions(api, manifest, translation, config, "bee-houses", () => config.OtherInteractions.BeeHouses, ConfigColor.DefaultBeeHouse);
            RegisterOtherInteractionOptions(api, manifest, translation, config, "mushroom-boxes", () => config.OtherInteractions.MushroomBoxes, ConfigColor.DefaultMushroomBox);
            RegisterOtherInteractionOptions(api, manifest, translation, config, "garbage-cans", () => config.OtherInteractions.GarbageCans, ConfigColor.DefaultGarbageCan, showWitnessBlock: true);
            RegisterOtherInteractionOptions(api, manifest, translation, config, "hay-grass", () => config.OtherInteractions.HayGrass, ConfigColor.DefaultHay);
        }

        private static void RegisterOtherInteractionOptions(
            IGenericModConfigMenuApi api,
            IManifest manifest,
            ITranslationHelper translation,
            ModConfig config,
            string key,
            Func<InteractionRule> getRule,
            string defaultColor,
            bool showWitnessBlock = false)
        {
            api.AddSectionTitle(manifest, () => translation.Get($"config.other-interactions.{key}.section"));

            string prefix = $"OtherInteractions.{ToPascalKey(key)}";

            if (showWitnessBlock)
            {
                api.AddBoolOption(
                    mod: manifest,
                    getValue: () => getRule().BlockWhenWitnessed,
                    setValue: value => getRule().BlockWhenWitnessed = value,
                    name: () => translation.Get("config.other-interactions.block-when-witnessed.name"),
                    tooltip: () => translation.Get($"config.other-interactions.{key}.block-when-witnessed.tooltip"),
                    fieldId: $"{prefix}.BlockWhenWitnessed");
            }

            api.AddBoolOption(
                mod: manifest,
                getValue: () => getRule().Auto,
                setValue: value => getRule().Auto = value,
                name: () => translation.Get("config.other-interactions.auto.name"),
                tooltip: () => translation.Get($"config.other-interactions.{key}.auto.tooltip"),
                fieldId: $"{prefix}.Auto");

            api.AddBoolOption(
                mod: manifest,
                getValue: () => getRule().Manual,
                setValue: value => getRule().Manual = value,
                name: () => translation.Get("config.other-interactions.manual.name"),
                tooltip: () => translation.Get($"config.other-interactions.{key}.manual.tooltip"),
                fieldId: $"{prefix}.Manual");

            api.AddBoolOption(
                mod: manifest,
                getValue: () => getRule().ShowLines,
                setValue: value => getRule().ShowLines = value,
                name: () => translation.Get("config.other-interactions.show-lines.name"),
                tooltip: () => translation.Get($"config.other-interactions.{key}.show-lines.tooltip"),
                fieldId: $"{prefix}.ShowLines");

            RegisterLineColorOption(
                api,
                manifest,
                translation,
                () => getRule().LineColor,
                value => getRule().LineColor = value,
                $"config.other-interactions.{key}.line-color.name",
                defaultColor);
        }

        private static string ToPascalKey(string key)
        {
            return key switch
            {
                "crab-pots" => "CrabPots",
                "fruit-trees" => "FruitTrees",
                "machines" => "Machines",
                "tappers" => "Tappers",
                "bee-houses" => "BeeHouses",
                "mushroom-boxes" => "MushroomBoxes",
                "garbage-cans" => "GarbageCans",
                "hay-grass" => "HayGrass",
                _ => key
            };
        }

        private static void RegisterLineColorOption(
            IGenericModConfigMenuApi api,
            IManifest manifest,
            ITranslationHelper translation,
            Func<string> getValue,
            Action<string> setValue,
            string nameKey,
            string defaultValue)
        {
            api.AddTextOption(
                mod: manifest,
                getValue: getValue,
                setValue: value => setValue(ConfigColor.SanitizeValue(value, defaultValue)),
                name: () => translation.Get(nameKey),
                tooltip: () => translation.Get("config.color-line.tooltip"));
        }

        private static void RegisterItemsPage(
            IGenericModConfigMenuApi api,
            IModHelper helper,
            ModConfig config,
            IManifest manifest)
        {
            ITranslationHelper translation = helper.Translation;

            api.AddPage(manifest, "items", () => translation.Get("config.page.items.title"));

            api.AddParagraph(manifest, () => translation.Get("config.page.items.description"));

            RegisterItemTypeOptions(api, manifest, translation, "ground-forage", () => config.ItemRules.GroundForage);
            RegisterItemTypeOptions(api, manifest, translation, "bushes", () => config.ItemRules.Bushes);
            RegisterItemTypeOptions(api, manifest, translation, "artifact-spots", () => config.ItemRules.ArtifactSpots);
            RegisterItemTypeOptions(api, manifest, translation, "panning", () => config.ItemRules.Panning);
        }

        private static void RegisterItemTypeOptions(
            IGenericModConfigMenuApi api,
            IManifest manifest,
            ITranslationHelper translation,
            string key,
            Func<ScopeRule> getRule)
        {
            api.AddSectionTitle(
                mod: manifest,
                text: () => translation.Get($"config.items.{key}.section"));

            string autoFieldId = $"ItemRules.{key}.Auto";
            string manualFieldId = $"ItemRules.{key}.Manual";

            api.AddBoolOption(
                mod: manifest,
                getValue: () => getRule().Auto,
                setValue: value => getRule().Auto = value,
                name: () => translation.Get("config.items.auto.name"),
                tooltip: () => translation.Get($"config.items.{key}.auto.tooltip"),
                fieldId: autoFieldId);

            api.AddBoolOption(
                mod: manifest,
                getValue: () => getRule().Manual,
                setValue: value => getRule().Manual = value,
                name: () => translation.Get("config.items.manual.name"),
                tooltip: () => translation.Get($"config.items.{key}.manual.tooltip"),
                fieldId: manualFieldId);
        }

        private static void RegisterAreasPage(
            IGenericModConfigMenuApi api,
            IModHelper helper,
            ModConfig config,
            IManifest manifest)
        {
            ITranslationHelper translation = helper.Translation;

            api.AddPage(manifest, "areas", () => translation.Get("config.page.areas.title"));

            api.AddParagraph(manifest, () => translation.Get("config.page.areas.description"));

            api.AddSectionTitle(manifest, () => translation.Get("config.areas.locations.section"));

            foreach (string locationId in LocationCatalog.LocationIds)
            {
                string capturedId = locationId;

                api.AddSectionTitle(
                    mod: manifest,
                    text: () => LocationCatalog.GetDisplayName(capturedId));

                api.AddBoolOption(
                    mod: manifest,
                    getValue: () => config.Areas.IsAutoBlocked(capturedId),
                    setValue: value => config.Areas.SetAutoBlocked(capturedId, value),
                    name: () => translation.Get("config.areas.block-auto.name"),
                    tooltip: () => translation.Get("config.areas.location-block-auto.tooltip"),
                    fieldId: $"Areas.Blocked.{capturedId}.Auto");

                api.AddBoolOption(
                    mod: manifest,
                    getValue: () => config.Areas.IsManualBlocked(capturedId),
                    setValue: value => config.Areas.SetManualBlocked(capturedId, value),
                    name: () => translation.Get("config.areas.block-manual.name"),
                    tooltip: () => translation.Get("config.areas.location-block-manual.tooltip"),
                    fieldId: $"Areas.Blocked.{capturedId}.Manual");
            }
        }
    }

}
