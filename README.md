# 7D2D Affix System

Barebones affix prototype for 7 Days to Die V3.0.

This pass supports command-spawned affixed weapons and tools, fresh generated loot rolls,
and a prototype augment currency for adding one new affix to an existing Magic
or Rare item.

## Features

- Adds a Harmony-backed modlet.
- Adds a cheat command for spawning Magic or Rare weapons and tools.
- Rolls Magic or Rare affixes onto newly generated loot container and loot bag
  weapons and tools.
- Stores affix state on the item instance.
- Applies affix values through `ItemValue` stat boosts.
- Adds a star Affixes tab next to Stats and Description in the item info panel.
- Shows affix rarity, affix names, tier colors, and rolled values in that tab.
- Adds a prototype `Affix Augment` currency item with an inventory use action.
- Keeps weapon affix pools scoped by item category so melee-only and gun-only
  stats do not roll on the wrong weapon type.
- Adds the first tool-specific affix pool for block damage, harvest count,
  stamina cost, durability, motor tools, and lower-value entity damage.

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

## Test Commands

Open the F1 console in a world and run:

```text
affix spawn rare
```

Open the item info panel for the spawned item and click the star icon beside
Stats and Description to inspect rolled affixes.

Usage:

```text
affix spawn <magic|rare> [itemName=gunHandgunT1Pistol] [quality=6] [drop=false]
affix inspect
affix currency [count=1]
affix augment
affix list [all|held]
affix validate [itemName] [quality=6]
affix debug loot <on|off>
affix reload
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
affix spawn rare meleeToolPickT1IronPickaxe 6
affix spawn rare meleeToolPickT3Auger 6
affix spawn rare gunHandgunT1Pistol 6 true
affix inspect
affix currency 3
affix augment
affix list held
affix validate meleeWpnBladeT1HuntingKnife 5
affix validate meleeToolPickT1IronPickaxe 6
affix validate meleeToolPickT3Auger 6
affix debug loot on
```

`drop=false` adds the item to the backpack. `drop=true` drops it at the player.

`Affix Augment` can be used from inventory while the target Magic or Rare item
is selected in the toolbelt. The `affix augment` command does the same thing as
a console fallback and consumes one `Affix Augment` from the toolbelt or
backpack. By default, generated Magic items roll 2 affixes and can be augmented
up to 3. Generated Rare items roll 4 affixes and can be augmented up to 6.

Use `affix list held` or `affix validate` to inspect which affixes can still
roll on the held item. Use `affix validate <itemName> <quality>` to inspect a
specific base item without spawning it.

## Tuning

Runtime tuning lives in:

```text
Config/affix_tuning.xml
```

The file controls generated-loot rolling, loot debug logging, Magic/Rare rarity
weights, natural affix counts, augment caps, and the augment currency item name.
After editing the file in a running world, use:

```text
affix reload
```

## License

This project is licensed under the [MIT License](LICENSE).
