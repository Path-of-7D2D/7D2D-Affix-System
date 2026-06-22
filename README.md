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
- Shows affix rarity, affix names, tiers, and rolled values in the item info
  panel.

## Install

Build the project:

```sh
dotnet build src/AffixSystem/AffixSystem.csproj -c Release
```

The build copies `AffixSystem.dll` into `1A-AffixSystem/`.

If the game is installed at the default Steam path, the build also refreshes the
live game install:

```text
7 Days To Die/Mods/1A-AffixSystem/
```

EasyAntiCheat must be disabled.

## Test Command

Open the F1 console in a world and run:

```text
affixspawn rare
```

Usage:

```text
affixspawn <magic|rare> [itemName=gunHandgunT1Pistol] [quality=6] [drop=false]
```

Alias:

```text
affixgive
```

Examples:

```text
affixspawn magic
affixspawn rare gunHandgunT1Pistol 6
affixspawn rare meleeWpnBladeT1HuntingKnife 5
affixspawn rare gunHandgunT1Pistol 6 true
```

`drop=false` adds the item to the backpack. `drop=true` drops it at the player.
