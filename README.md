## Description and Features

**Forage Automator** is a [Stardew Valley](http://stardewvalley.net/) SMAPI mod that automatically collects forageables on the current map.

### Auto collect on range

Walk near forageables and they are collected automatically — no hotkey required. Radius is configurable in Generic Mod Config Menu. Scans more often while moving or at high speed, and snaps to targets when needed.

### Auto collect whole map

When enabled, automatically sweeps the current map for forageables **whenever you enter a location**.

### Hotkey sweeps

- **F5** (configurable) — start or cancel a sweep within pickup radius
- **F6** (configurable) — start or cancel a whole-map sweep on the current location

Press the same hotkey again to cancel an active sweep.

### Pathfinding

When **Use pathfinding** is enabled, sweeps walk to each target. When disabled (or when moving very fast), the player snaps to each stand tile before collecting — recommended for 10x+ speed mods.

### Return to start

Optional **Return to start after sweep** moves you back to where you stood when the sweep started when it finishes or is cancelled. Off by default.

### Target lines

When a forageable cannot be picked up (full inventory, missing tool, unreachable), a colored line is drawn from the player to the target. Lines are hidden while a sweep is running.

### Supported forageables

Does **not** collect regular farm crops (parsnips, melons, wheat, etc.).

| Type           | Examples                                      | Tool                    | Config toggle  |
| :------------- | :-------------------------------------------- | :---------------------- | :------------- |
| Ground forage  | Seasonal spawns, `forage_item` tagged objects | None                    | Ground forage  |
| Forage crops   | Spring onions, ginger                         | Ginger needs a hoe      | Ground forage  |
| Bushes         | Berry bushes, tea bushes                      | None                    | Berry bushes   |
| Artifact spots | Worm/seed tiles                               | Hoe in inventory        | Artifact spots |
| Panning spots  | Glittering water tiles                        | Copper pan in inventory | Panning spots  |

## Installation

1. [Install the latest version of SMAPI](https://smapi.io/).
2. Build or download the mod release.
3. Unzip the mod folder into your `Stardew Valley/Mods` directory.
4. Run the game using SMAPI.

## Configuration

If you have [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098) installed, open **Mod Options** from the title screen or in-game menu.

| Setting                                           | Default        | Description                                                              |
| :------------------------------------------------ | :------------- | :----------------------------------------------------------------------- |
| Auto collect on range                             | off            | Auto-collect forageables within pickup radius while walking              |
| Pickup radius                                     | 2 tiles        | Proximity range for range pickup and range sweeps                        |
| Auto collect whole map                            | off            | Auto-sweep when entering a location                                      |
| Range sweep hotkey                                | F5             | Start/cancel sweep within pickup radius                                  |
| Whole map sweep hotkey                            | F6             | Start/cancel whole-map sweep                                             |
| Return to start after sweep                       | off            | Return to start tile when a sweep finishes or is cancelled               |
| Use pathfinding                                   | on             | Walk to targets during sweep; off = instant snap (better for speed mods) |
| Show target lines                                 | on             | Draw lines to unpicked forageables                                       |
| Line range                                        | 0 (entire map) | Max distance for target lines                                            |
| Ground forage / bushes / artifact spots / panning | on             | Which target types to collect                                            |
| Show XP in sweep summary                          | on             | Include XP gained in sweep-complete message                              |
| Show HUD messages                                 | on             | On-screen notifications                                                  |
| Notify when inventory full                        | on             | Toast when inventory blocks pickup                                       |
| Notify when tool missing                          | on             | Toast when hoe/pan is missing                                            |

GMCM also exposes per-type line toggles, line colors, and riding-horse notifications.

## Console Commands

| Command     | Description                          |
| :---------- | :----------------------------------- |
| `fa_start`  | Start a full-map forage sweep        |
| `fa_stop`   | Cancel the active sweep              |
| `fa_status` | Show sweep and skipped-target status |

## Compatibility

Forage Automator targets Stardew Valley 1.6+. Single-player is fully supported; multiplayer is untested.

## Developer Info

```bash
dotnet build ForageAutomator.sln
```

Requires the [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) and a local Stardew Valley install path in `ForageAutomator.csproj`.
