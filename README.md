# 7D2D Affix System

Barebones affix prototype for 7 Days to Die V3.0.

This pass supports command-spawned affixed weapons, tools, and armor, fresh generated loot rolls,
and a prototype augment currency for adding one new affix to an existing Magic
or Rare item.

## Features

- Adds a Harmony-backed modlet.
- Adds a cheat command for spawning Magic or Rare weapons, tools, and armor.
- Rolls Magic or Rare affixes onto newly generated loot container and loot bag
  weapons, tools, and armor.
- Stores affix state on the item instance.
- Records the affix roll origin on newly written affix metadata for debugging
  found-loot versus command-spawned items.
- Applies affix values through `ItemValue` stat boosts.
- Adds a star Affixes tab next to Stats and Description in the item info panel.
- Shows affix rarity, affix names, tier colors, and rolled values in that tab.
- Displays Magic item names with their leading affix and Rare item names with
  affix-derived prefix/suffix identity.
- Adds a prototype `Affix Augment` currency item with an inventory use action.
- Reports the added affix and resulting affix count/cap when an augment
  succeeds.
- Keeps weapon affix pools scoped by item category so melee-only and gun-only
  stats do not roll on the wrong weapon type.
- Adds bow/crossbow-specific affixes for projectile velocity, bow damage, and
  crossbow reload speed.
- Adds the first tool-specific affix pool for block damage, harvest count,
  stamina cost, durability, motor tools, and lower-value entity damage.
- Adds the first armor-specific affix pool for health, stamina, movement,
  stealth, resistances, carry capacity, and conservative global damage.
- Applies per-affix quality gates so higher-power affix families start appearing
  on better item bases.
- Prevents duplicate affix families on a single item during natural rolls and
  augment rolls.
- Adds controlled `Affix Augment` drops to high-value found loot such as weapon
  bags, toolboxes, armor racks, reinforced/hardened chests, infestation loot,
  zombie loot bags, and low-probability weapon/tool/armor quest reward pools.

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
affix inspect [raw]
affix currency [count=1]
affix augment
affix list [all|held]
affix validate [itemName] [quality=6]
affix rolltest <itemName> [quality=6] [samples=1000] [source]
affix debug loot <on|off>
affix debug rarity [source] [quality=6]
affix reload
```

Inspect the currently held toolbelt item:

```text
affix inspect
```

Use `affix inspect raw` when testing persistence. It prints the item type,
quality, seed, raw affix metadata, stored rarity/count, cap, legal remaining
affix count, and metadata origin for the held item.

Examples:

```text
affix spawn magic
affix spawn rare gunHandgunT1Pistol 6
affix spawn rare gunBowT1WoodenBow 6
affix spawn rare gunBowT1IronCrossbow 6
affix spawn rare meleeWpnBladeT1HuntingKnife 5
affix spawn rare meleeToolPickT1IronPickaxe 6
affix spawn rare meleeToolPickT3Auger 6
affix spawn rare armorPrimitiveHelmet 6
affix spawn rare armorMinerOutfit 6
affix spawn rare gunHandgunT1Pistol 6 true
affix inspect
affix inspect raw
affix currency 3
affix augment
affix list held
affix validate meleeWpnBladeT1HuntingKnife 5
affix validate gunBowT1WoodenBow 6
affix validate gunBowT1IronCrossbow 6
affix validate meleeToolPickT1IronPickaxe 6
affix validate meleeToolPickT3Auger 6
affix validate armorPrimitiveHelmet 6
affix validate armorMinerOutfit 6
affix rolltest gunHandgunT1Pistol 6 1000 container:hardenedChestT5
affix debug loot on
affix debug rarity container:hardenedChestT5 6
```

`drop=false` adds the item to the backpack. `drop=true` drops it at the player.

`Affix Augment` can be used from inventory while the target Magic or Rare item
is selected in the toolbelt. The `affix augment` command does the same thing as
a console fallback and consumes one `Affix Augment` from the toolbelt or
backpack. By default, generated Magic items roll 1-2 affixes based on quality
and can be augmented up to 3. Generated Rare items roll 2-6 affixes based on
quality and can be augmented up to 6.

Use `affix list held` or `affix validate` to inspect which affixes can still
roll on the held item. Use `affix validate <itemName> <quality>` to inspect a
specific base item without spawning it. `affix list all` also shows each
affix's duplicate-prevention family and quality gate.

Use `affix rolltest <itemName> <quality> <samples> [source]` to simulate
natural loot rolls without creating items. The output reports effective
Magic/Rare weights, rarity results, average affix counts, average tier, and the
most common rolled affix IDs.

## Tuning

Runtime tuning lives in:

```text
Config/affix_tuning.xml
```

The file controls generated-loot rolling, loot debug logging, Magic/Rare rarity
weights, high-risk loot source rarity bias, quality-based natural affix count
ranges, augment caps, and the augment currency item name.
Natural roll counts are clamped by the configured Magic/Rare caps.
After editing the file in a running world, use:

```text
affix reload
```

Default generated loot uses quality-based Magic/Rare weights: Q1-2 use 80/20,
Q3-4 use 70/30, and Q5-6 use 60/40. Sources matching the configured high-risk
patterns, such as hardened or high-tier infested loot, move 10 weight points
from Magic to Rare by default. Use `affix debug rarity container:hardenedChestT5
6` to inspect the effective weights for a source string and quality.

Currency acquisition is patched through:

```text
Config/loot.xml
```

That file controls where `Affix Augment` can appear in loot. It intentionally
does not add normal trader stock.

## License

This project is licensed under the [MIT License](LICENSE).
