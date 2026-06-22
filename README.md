# 7D2D Affix System

Barebones affix prototype for 7 Days to Die V3.0.

This first pass focuses on command-spawned affixed weapons so the storage,
display, and stat-modification pipeline can be tested before loot generation is
hooked up.

## Features

- Adds a Harmony-backed modlet.
- Adds a cheat command for spawning Magic or Rare weapons.
- Stores affix state on the item instance.
- Applies affix values through `ItemValue` stat boosts.
- Adds a star Affixes tab next to Stats and Description in the item info panel.
- Shows affix rarity, affix names, tier colors, and rolled values in that tab.

## Install

Build the project:

```sh
dotnet build src/AffixSystem/AffixSystem.csproj -c Release
```

The build copies `AffixSystem.dll` and XUi config patches into
`1A-AffixSystem/`.

If the game is installed at the default Steam path, the build also refreshes the
live game install:

```text
7 Days To Die/Mods/1A-AffixSystem/
```

EasyAntiCheat must be disabled.

## Test Command

Open the F1 console in a world and run:

```text
affix spawn rare
```

Open the item info panel for the spawned weapon and click the star icon beside
Stats and Description to inspect rolled affixes.

Usage:

```text
affix spawn <magic|rare> [itemName=gunHandgunT1Pistol] [quality=6] [drop=false]
```

Inspect the currently held toolbelt item:

```text
affix inspect
```

Examples:

```text
affix spawn magic
affix spawn rare gunHandgunT1Pistol 6
affix spawn rare meleeWpnBladeT1HuntingKnife 5
affix spawn rare gunHandgunT1Pistol 6 true
affix inspect
```

`drop=false` adds the item to the backpack. `drop=true` drops it at the player.

## License

This project is licensed under the [MIT License](LICENSE).
