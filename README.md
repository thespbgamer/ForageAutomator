## Description and Features

**Forage Automator** is a [Stardew Valley](http://stardewvalley.net/) SMAPI mod that automatically collects forageables on the current map. It works in single-player; multiplayer is untested.

### Auto collect on range

- **Behavior:** While enabled, forageables within your pickup radius are collected as you walk — no hotkey required.
- Scans more often while moving or at high speed, and snaps to targets when needed.

### Auto collect whole map

- **Behavior:** When enabled, automatically sweeps the current map for forageables whenever you enter a location.

### Range sweep hotkey

- **Keyboard:** Press `F5` (configurable) to start or cancel a forage sweep within pickup radius.

### Whole map sweep hotkey

- **Keyboard:** Press `F6` (configurable) to start or cancel a whole-map forage sweep on the current location.

### Pathfinding

- When **Use pathfinding** is enabled, sweeps walk to each target.
- When disabled (or when moving very fast), the player snaps to each stand tile before collecting — recommended for 10x+ speed mods.

### Return to start

- Optional **Return to start after sweep** moves you back to where you stood when the sweep started when it finishes or is cancelled. Off by default.

### Target lines

- When a forageable cannot be picked up (full inventory, missing tool, unreachable), a colored line is drawn from the player to the target.
- Lines are hidden while a sweep is running.

### Supported forageables

Does **not** collect regular farm crops (parsnips, melons, wheat, etc.).

| Type | Examples | Tool | Config toggle |
| :--- | :------- | :--- | :------------ |
| Ground forage | Seasonal spawns, `forage_item` tagged objects | None | `CollectGroundForage` |
| Forage crops | Spring onions, ginger | Hoe for ginger | `CollectGroundForage` |
| Bushes | Berry bushes, tea bushes | None | `CollectBushes` |
| Artifact spots | Worm/seed tiles | Hoe | `CollectArtifactSpots` |
| Panning spots | Glittering water tiles | Pan | `CollectPanning` |

> [!NOTE]
> Forage crops (spring onions, ginger) are vanilla crops flagged as forage — not regular planted farm crops. Ginger needs a hoe; spring onions are hand-picked. Any hoe or pan in your inventory works (not a specific tier).

## Contents

- [Description and Features](#description-and-features)
- [Contents](#contents)
- [Installation](#installation)
- [Console Commands](#console-commands)
  - [In-game settings](#in-game-settings)
- [Configuration](#configuration)
  - [In-game settings](#in-game-settings-1)
  - [`config.json` file](#configjson-file)
- [Compatibility](#compatibility)
- [Translation Progress](#translation-progress)
- [Changelog](#changelog)
- [Developer Info / SDK Installation](#developer-info--sdk-installation)
- [See also](#see-also)

## Installation

1. [Install the latest version of SMAPI](https://smapi.io/).
2. Download the mod from GitHub releases or build it locally (see [Developer Info](#developer-info--sdk-installation)).
3. Unzip the mod folder into your `Stardew Valley/Mods` directory.
4. Run the game using SMAPI.

## Console Commands

### In-game settings

Once you have the game running (having a save loaded), you can use any of these commands at any time.

Here's what you can do:

| Command Name | Optional Parameter(s) | Description |
| :--------- | :-------------------- | :---------- |
| `fa_start` | `null` aka **nothing** | Starts a full-map forage sweep on the current location. |
| `fa_stop` | `null` aka **nothing** | Cancels the active forage sweep. |
| `fa_status` | `null` aka **nothing** | Shows sweep state, queue size, and skipped-target count. |

## Configuration

### In-game settings

If you have the [generic mod config menu](https://www.nexusmods.com/stardewvalley/mods/5098?tab=files) installed, the configuration process becomes much simpler. You can click the cog button (⚙) on the title screen or the **mod options** button at the bottom of the in-game menu to configure the mod.

### `config.json` file

The mod creates a `config.json` file in its mod folder the first time you run it. You can open the file in a text editor like Notepad to configure the mod.

Here's what you can change:

- Range pickup and sweeps:

  | Setting Name | Default Value | Description |
  | :---------------------- | :------------ | :---------------------------------------------------------------- |
  | `AutoCollectOnRange` | `false` | Auto-collect forageables within pickup radius while walking. |
  | `PickupRadius` | `2` | Pickup radius in tiles (range pickup and range sweeps). |
  | `AutoCollectWholeMap` | `false` | Auto-sweep when entering a location. |
  | `RangeKey` | `F5` | Start or stop a forage sweep within pickup radius. |
  | `WholeMapKey` | `F6` | Start or stop a whole-map forage sweep. |
  | `ReturnToStartAfterSweep` | `false` | Return to start tile when a sweep finishes or is cancelled. |
  | `UsePathfinding` | `true` | Walk to targets during sweep; `false` = instant snap. |
  | `ShowSweepExperience` | `true` | Include XP gained in the sweep-complete HUD message. |

- Collect target types:

  | Setting Name | Default Value | Description |
  | :--------------------- | :------------ | :-------------------------------------------------------------- |
  | `CollectGroundForage` | `true` | Ground forage, spring onions, and similar forage crops. |
  | `CollectBushes` | `true` | Berry bushes and tea bushes. |
  | `CollectArtifactSpots` | `true` | Artifact and seed spots (hoe required). |
  | `CollectPanning` | `true` | Panning spots (pan required). |

- Target lines:

  | Setting Name | Default Value | Description |
  | :------------------------ | :------------ | :---------------------------------------------------- |
  | `ShowTargetLines` | `true` | Draw lines to forageables that were not picked up. |
  | `LineRange` | `0` | Max tile distance for lines; `0` = entire current map. |
  | `ShowLinesGroundForage` | `true` | Lines to ground forage and forage crops. |
  | `ShowLinesBushes` | `true` | Lines to berry and tea bushes. |
  | `ShowLinesEmptyBushes` | `false` | Lines to in-season bushes with no berries yet. |
  | `ShowLinesArtifactSpots` | `true` | Lines to artifact and seed spots. |
  | `ShowLinesPanning` | `true` | Lines to panning spots. |

- Line colors (RGBA, comma-separated):

  | Setting Name | Default Value | Description |
  | :---------------------- | :---------------- | :-------------------------------- |
  | `ColorLineReady` | `80,255,100,200` | Ready to collect (in range). |
  | `ColorLineOutOfRange` | `80,180,255,200` | In range of map but outside pickup radius. |
  | `ColorLineMissingTool` | `255,210,50,220` | Missing required hoe or pan. |
  | `ColorLineInventoryFull` | `255,80,80,220` | Inventory full. |
  | `ColorLineUnreachable` | `160,160,160,200` | No valid path to collect. |
  | `ColorLineEmptyBush` | `120,120,120,160` | In-season bush with no berries. |

- HUD messages:

  | Setting Name | Default Value | Description |
  | :------------------- | :------------ | :---------------------------------------------- |
  | `ShowHudMessages` | `true` | Show on-screen notifications. |
  | `NotifyInventoryFull` | `true` | Toast when inventory blocks pickup. |
  | `NotifyMissingTool` | `true` | Toast when a required tool is missing. |
  | `NotifyRidingHorse` | `true` | Toast when a sweep is blocked while riding a horse. |

## Compatibility

Forage Automator is compatible with Stardew Valley 1.6+ on Linux/Mac/Windows. Single-player is fully supported; multiplayer is untested.

Requires [SMAPI](https://smapi.io/) 4.5.2 or later. [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098) is optional but recommended.

## Translation Progress

The mod can be translated into any language supported by the game.

Contributions are welcome! See [Modding:Translations](https://stardewvalleywiki.com/Modding:Translations) on the wiki for help contributing translations.

(❑ = untranslated, ↻ = partly translated, ✓ = fully translated)

| Language | Status | Code |
| :------- | :----- | :--- |
| English | [✓](ForageAutomator/i18n/default.json) | `default.json` and `en.json` |
| Chinese | [❑](ForageAutomator/i18n/zh.json) | `zh.json` |
| French | [❑](ForageAutomator/i18n/fr.json) | `fr.json` |
| German | [❑](ForageAutomator/i18n/de.json) | `de.json` |
| Hungarian | [❑](ForageAutomator/i18n/hu.json) | `hu.json` |
| Italian | [❑](ForageAutomator/i18n/it.json) | `it.json` |
| Japanese | [❑](ForageAutomator/i18n/ja.json) | `ja.json` |
| Korean | [❑](ForageAutomator/i18n/ko.json) | `ko.json` |
| [Polish] | [❑](ForageAutomator/i18n/pl.json) | `pl.json` |
| Portuguese | [❑](ForageAutomator/i18n/pt.json) | `pt.json` |
| Russian | [❑](ForageAutomator/i18n/ru.json) | `ru.json` |
| Spanish | [❑](ForageAutomator/i18n/es.json) | `es.json` |
| [Thai] | [❑](ForageAutomator/i18n/th.json) | `th.json` |
| Turkish | [❑](ForageAutomator/i18n/tr.json) | `tr.json` |
| [Ukrainian] | [❑](ForageAutomator/i18n/uk.json) | `uk.json` |

[Polish]: https://www.nexusmods.com/stardewvalley/mods/3616
[Thai]: https://www.nexusmods.com/stardewvalley/mods/7052
[Ukrainian]: https://www.nexusmods.com/stardewvalley/mods/8427

## Changelog

- [Full Changelog](CHANGELOG.md#changelog)

## Developer Info / SDK Installation

If you plan to build or modify **Forage Automator** yourself, you'll need the **.NET 6 SDK** installed.

```bash
dotnet build ForageAutomator.sln
```

Set your local Stardew Valley install path in `ForageAutomator/ForageAutomator.csproj` (`GamePath`) before building.

For more detailed instructions and other platforms, see the official Microsoft documentation: [Install .NET SDK](https://learn.microsoft.com/en-us/dotnet/core/install/).

## See also

- Nexus mod — not available yet
- CurseForge mod — not available yet
