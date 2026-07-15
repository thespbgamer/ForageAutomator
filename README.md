## Description and Features

**Forage Automator** is a [Stardew Valley](http://stardewvalley.net/) SMAPI mod that automatically collects forageables on the current map. It works in single-player; multiplayer is untested.

Collection happens in two modes:

| Mode          | What triggers it                                                              |
| :------------ | :---------------------------------------------------------------------------- |
| **Automatic** | Range pickup while walking, and whole-map auto-sweep when entering a location |
| **Manual**    | `F5` / `F6` hotkeys, and the `fa_start` console command                       |

Item types and locations can be configured separately for each mode. Changes apply immediately in-game.

### Auto collect on range

- While enabled, forageables within your pickup radius are collected as you walk — no hotkey required.
- Scans more often while moving or at high speed, and snaps to targets when needed.
- Respects **Item rules** (automatic) and **Area rules** (automatic blocklist).

### Auto collect whole map

- When enabled, automatically sweeps the current map for forageables when you enter a location (or when new forage appears).
- Does not run during cutscenes, dialogue, menus, bus travel animations, or while the player has a movement controller.
- Respects **Item rules** (automatic) and **Area rules** (automatic blocklist).
- Shows _Automatic forage sweep blocked in this area_ only when the area blocks auto collection **and** there are collectable targets on the map.

### Range sweep hotkey

- Press `F5` (configurable) to start or cancel a forage sweep within pickup radius.
- Uses **Manual** item and area rules.

### Whole map sweep hotkey

- Press `F6` (configurable) to start or cancel a whole-map forage sweep on the current location.
- Uses **Manual** item and area rules.

### Pathfinding

- When **Use pathfinding** is enabled, sweeps walk to each target.
- When disabled (or when moving very fast), the player snaps to each stand tile before collecting — recommended for 10x+ speed mods.
- Reachability is not calculated when pathfinding is off or at high speed, which avoids an expensive full-map flood fill that would not be used anyway.

### Return to start

- Optional **Return to start after sweep** moves you back to where you stood when the sweep started when it finishes or is cancelled. Off by default.

### Target lines

- When a forageable cannot be picked up (full inventory, missing tool, unreachable, on horse, etc.), a colored line is drawn from the player to the target.
- Lines are hidden while a sweep is running.
- Line visibility is controlled per forage type; line colors are configurable.

### Item rules

Per forage type, choose whether **automatic** and **manual** collection are enabled:

| Type           | Examples                                      | Tool           |
| :------------- | :-------------------------------------------- | :------------- |
| Ground forage  | Seasonal spawns, `forage_item` tagged objects | None           |
| Forage crops   | Spring onions, ginger                         | Hoe for ginger |
| Bushes         | Berry bushes, tea bushes                      | None           |
| Artifact spots | Worm/seed tiles                               | Hoe            |
| Panning spots  | Glittering water tiles                        | Pan            |

All types default to **on** for both automatic and manual collection.

> [!NOTE]
> Forage crops (spring onions, ginger) are vanilla crops flagged as forage — not regular planted farm crops. Ginger needs a hoe; spring onions are hand-picked. Any hoe or pan in your inventory works (not a specific tier).

> [!NOTE]
> Does **not** collect regular farm crops (parsnips, melons, wheat, etc.).

### Other interactions

Optional non-forage interactions on the **Other interactions** GMCM page. All types default to **off** for automatic collection, manual sweeps, and target lines.

| Type | What it does | Tool |
| :--- | :----------- | :--- |
| Crab pots | Harvest ready pots | None |
| Fruit trees | Shake when fruit is ready | None |
| Processing machines | Keg, preserves jar, cheese press, loom, etc. | None |
| Tappers | Normal and heavy tapper output | None |
| Bee houses | Honey when ready | None |
| Mushroom boxes & logs | Ready mushroom output | None |
| Garbage cans | Once-per-day loot (per can) via vanilla `CheckGarbage` | None |
| Hay (grass) | Cut grass for hay | Scythe |

Garbage cans use the same loot rolls as clicking them yourself (vanilla `CheckGarbage`). Each can can only be checked **once per day** (`CheckedGarbage`). Loot spawns as ground debris; the mod pauses briefly at the can to pick it up (including when moving fast).

**Skip when witnessed** (garbage cans only, default **on**): skip cans when a villager would see you dumpster dive (15×15 tile area around the player, using each NPC's dumpster-dive friendship effect from game data). Turn off to loot anyway — vanilla friendship loss still applies. Target lines use the NPC witness color when skip-when-witnessed is on and someone is watching.

Each type has its own line color when ready to collect.

### Area rules (blocklist)

Collection is **allowed everywhere by default**. Turn on a block to exclude a location.

| Block                          | Affects                               |
| :----------------------------- | :------------------------------------ |
| **Block automatic collection** | Range pickup and whole-map auto-sweep |
| **Block manual collection**    | Hotkey sweeps and `fa_start`          |

Only locations where forage can appear are listed (outdoor maps, forage spawn data, greenhouses, farm cave, mines, etc.).

**Mines default:** automatic collection is blocked in mines (`Mine`, `UndergroundMine` floors such as `UndergroundMine42`, `SkullCave`, `VolcanoDungeon`, etc.). Manual sweeps are still allowed unless you block them. One **UndergroundMine** entry in config applies to all mine floors.

## Contents

- [Description and Features](#description-and-features)
- [Contents](#contents)
- [Installation](#installation)
- [Console Commands](#console-commands)
- [Configuration](#configuration)
  - [In-game settings (GMCM)](#in-game-settings-gmcm)
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

Load a save first. Then use:

| Command     | Description                                                            |
| :---------- | :--------------------------------------------------------------------- |
| `fa_start`  | Starts a full-map forage sweep on the current location (manual rules). |
| `fa_stop`   | Cancels the active forage sweep.                                       |
| `fa_status` | Shows sweep state, queue size, and skipped-target count.               |

## Configuration

### In-game settings (GMCM)

If you have [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098) installed, open the mod config from the title-screen cog (⚙) or **Mod options** in the pause menu.

The config has four pages (links on the root screen):

| Page                   | Contents                                                                   |
| :--------------------- | :------------------------------------------------------------------------- |
| **Main options**       | Range pickup, sweeps, pathfinding, target lines, line colors, HUD messages |
| **Item rules**         | Per-type automatic / manual collection toggles                             |
| **Area rules**         | Per-location block automatic / block manual toggles                        |
| **Other interactions** | Crab pots, machines, garbage cans, and other non-forage automations        |

#### Main options

| Setting                      | Default   | Description                                                   |
| :--------------------------- | :-------- | :------------------------------------------------------------ |
| Auto collect on range        | `false`   | Passive pickup within pickup radius while walking.            |
| Pickup radius (tiles)        | `2`       | Radius for range pickup and range sweeps.                     |
| Auto collect whole map       | `false`   | Auto-sweep when entering a location.                          |
| Range sweep hotkey           | `F5`      | Start/stop range sweep.                                       |
| Whole map sweep hotkey       | `F6`      | Start/stop whole-map sweep.                                   |
| Use pathfinding              | `true`    | Walk to targets; `false` = snap (better for high speed mods). Skips reachability calculation when off or at high speed. |
| Return to start after sweep  | `false`   | Return to start tile when a sweep ends or is cancelled.       |
| Show target lines            | `true`    | Draw lines to forageables that were not picked up.            |
| Line range (tiles)           | `0`       | Max distance for lines; `0` = entire current map.             |
| Lines: ground forage         | `true`    | Lines to ground forage and forage crops.                      |
| Lines: berry bushes          | `true`    | Lines to berry and tea bushes.                                |
| Lines: empty berry bushes    | `false`   | Lines to in-season bushes with no berries yet.                |
| Lines: artifact spots        | `true`    | Lines to artifact and seed spots.                             |
| Lines: panning spots         | `true`    | Lines to panning spots.                                       |
| Line colors                  | see below | RGBA comma-separated values per line state.                   |
| Show HUD messages            | `true`    | Master toggle for on-screen notifications.                    |
| Notify when inventory full   | `true`    | Toast when inventory blocks pickup.                           |
| Notify when tool missing     | `true`    | Toast when a required tool is missing.                        |
| Notify when riding horse     | `true`    | Toast when collection is blocked on horseback.                |
| Show XP in sweep summary     | `true`    | Include XP in the sweep-complete message.                     |
| Show sweep started message   | `true`    | Toast when a sweep starts.                                    |
| Show sweep cancelled message | `true`    | Toast when a sweep is cancelled.                              |

Default line colors (RGBA):

| State            | Default           |
| :--------------- | :---------------- |
| Ready to collect | `80,255,100,200`  |
| Out of range     | `80,180,255,200`  |
| Missing tool     | `255,210,50,220`  |
| Inventory full   | `255,80,80,220`   |
| Unreachable      | `160,160,160,200` |
| Empty berry bush | `120,120,120,160` |
| NPC witness      | `255,120,120,220` |

#### Item rules

Each forage type has **Automatic collection** and **Manual collection** toggles (all default `true`).

#### Area rules

Each listed location has **Block automatic collection** and **Block manual collection** toggles (default `false`, except mines block automatic collection by default).

#### Other interactions

Each non-forage type has **Automatic collection**, **Manual collection**, and **Show lines** toggles (all default `false`). Garbage cans also have **Skip when witnessed** (default `true`); see [Other interactions](#other-interactions) above.

### `config.json` file

The mod creates `config.json` in its mod folder on first run.

#### Range pickup and sweeps

| Key                         | Default | Description                   |
| :-------------------------- | :------ | :---------------------------- |
| `AutoCollectOnRange`        | `false` | Passive range pickup.         |
| `PickupRadius`              | `2`     | Pickup radius in tiles.       |
| `AutoCollectWholeMap`       | `false` | Auto-sweep on location enter. |
| `RangeKey`                  | `F5`    | Range sweep hotkey.           |
| `WholeMapKey`               | `F6`    | Whole-map sweep hotkey.       |
| `UsePathfinding`            | `true`  | Walk vs snap during sweeps. Skips reachability calculation when `false`. |
| `ReturnToStartAfterSweep`   | `false` | Return to start after sweep.  |
| `ShowSweepExperience`       | `true`  | XP in sweep-complete HUD.     |
| `ShowSweepStartedMessage`   | `true`  | Sweep-started HUD message.    |
| `ShowSweepCancelledMessage` | `true`  | Sweep-cancelled HUD message.  |

#### Item rules

```json
"ItemRules": {
  "GroundForage": { "Auto": true, "Manual": true },
  "Bushes": { "Auto": true, "Manual": true },
  "ArtifactSpots": { "Auto": true, "Manual": true },
  "Panning": { "Auto": true, "Manual": true }
}
```

#### Area rules (blocklist)

```json
"Areas": {
  "Blocked": {
    "UndergroundMine": { "Auto": true, "Manual": false }
  }
}
```

Only locations with a block entry (or mine defaults) are stored. `UndergroundMine` applies to all mine floors (`UndergroundMine1`, `UndergroundMine42`, etc.).

#### Target lines

| Key                      | Default           |
| :----------------------- | :---------------- |
| `ShowTargetLines`        | `true`            |
| `LineRange`              | `0`               |
| `ShowLinesGroundForage`  | `true`            |
| `ShowLinesBushes`        | `true`            |
| `ShowLinesEmptyBushes`   | `false`           |
| `ShowLinesArtifactSpots` | `true`            |
| `ShowLinesPanning`       | `true`            |
| `ColorLineReady`         | `80,255,100,200`  |
| `ColorLineOutOfRange`    | `80,180,255,200`  |
| `ColorLineMissingTool`   | `255,210,50,220`  |
| `ColorLineInventoryFull` | `255,80,80,220`   |
| `ColorLineUnreachable`   | `160,160,160,200` |
| `ColorLineEmptyBush`     | `120,120,120,160` |

NPC witness lines for garbage cans use `255,120,120,220` (not a separate config key).

#### Other interactions

Each entry under `OtherInteractions` has `Auto`, `Manual`, `ShowLines`, and `LineColor`. All default to `false` except line colors (per-type defaults in GMCM). Garbage cans also support `BlockWhenWitnessed` (default `true`).

```json
"OtherInteractions": {
  "CrabPots": { "Auto": false, "Manual": false, "ShowLines": false, "LineColor": "255,140,60,200" },
  "FruitTrees": { "Auto": false, "Manual": false, "ShowLines": false, "LineColor": "200,120,255,200" },
  "Machines": { "Auto": false, "Manual": false, "ShowLines": false, "LineColor": "160,200,255,200" },
  "Tappers": { "Auto": false, "Manual": false, "ShowLines": false, "LineColor": "180,140,80,200" },
  "BeeHouses": { "Auto": false, "Manual": false, "ShowLines": false, "LineColor": "255,220,80,200" },
  "MushroomBoxes": { "Auto": false, "Manual": false, "ShowLines": false, "LineColor": "140,200,120,200" },
  "GarbageCans": { "BlockWhenWitnessed": true, "Auto": false, "Manual": false, "ShowLines": false, "LineColor": "200,200,200,200" },
  "HayGrass": { "Auto": false, "Manual": false, "ShowLines": false, "LineColor": "220,180,100,200" }
}
```

#### HUD messages

| Key                   | Default |
| :-------------------- | :------ |
| `ShowHudMessages`     | `true`  |
| `NotifyInventoryFull` | `true`  |
| `NotifyMissingTool`   | `true`  |
| `NotifyRidingHorse`   | `true`  |

## Compatibility

Forage Automator is compatible with Stardew Valley 1.6+ on Linux/Mac/Windows. Single-player is fully supported; multiplayer is untested.

Requires [SMAPI](https://smapi.io/) 4.5.2 or later. [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098) is optional but recommended.

## Translation Progress

The mod can be translated into any language supported by the game.

Contributions are welcome! See [Modding:Translations](https://stardewvalleywiki.com/Modding:Translations) on the wiki.

(❑ = untranslated, ↻ = partly translated, ✓ = fully translated)

| Language    | Status                                 | Code                         |
| :---------- | :------------------------------------- | :--------------------------- |
| English     | [✓](ForageAutomator/i18n/default.json) | `default.json` and `en.json` |
| Chinese     | [❑](ForageAutomator/i18n/zh.json)      | `zh.json`                    |
| French      | [❑](ForageAutomator/i18n/fr.json)      | `fr.json`                    |
| German      | [❑](ForageAutomator/i18n/de.json)      | `de.json`                    |
| Hungarian   | [❑](ForageAutomator/i18n/hu.json)      | `hu.json`                    |
| Italian     | [❑](ForageAutomator/i18n/it.json)      | `it.json`                    |
| Japanese    | [❑](ForageAutomator/i18n/ja.json)      | `ja.json`                    |
| Korean      | [❑](ForageAutomator/i18n/ko.json)      | `ko.json`                    |
| [Polish]    | [❑](ForageAutomator/i18n/pl.json)      | `pl.json`                    |
| Portuguese  | [❑](ForageAutomator/i18n/pt.json)      | `pt.json`                    |
| Russian     | [❑](ForageAutomator/i18n/ru.json)      | `ru.json`                    |
| Spanish     | [❑](ForageAutomator/i18n/es.json)      | `es.json`                    |
| [Thai]      | [❑](ForageAutomator/i18n/th.json)      | `th.json`                    |
| Turkish     | [❑](ForageAutomator/i18n/tr.json)      | `tr.json`                    |
| [Ukrainian] | [❑](ForageAutomator/i18n/uk.json)      | `uk.json`                    |

[Polish]: https://www.nexusmods.com/stardewvalley/mods/3616
[Thai]: https://www.nexusmods.com/stardewvalley/mods/7052
[Ukrainian]: https://www.nexusmods.com/stardewvalley/mods/8427

## Changelog

- [Full Changelog](CHANGELOG.md#full-changelog)

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
