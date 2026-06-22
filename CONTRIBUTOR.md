# Contributor Guide

## Project Layout

```text
1A-AffixSystem/              deployable mod folder
  ModInfo.xml                mod manifest
  AffixSystem.dll            built Harmony mod assembly

src/AffixSystem/             C# source
  AffixSystem.csproj
  AffixSystemModApi.cs       IModApi entry point and Harmony bootstrap
```

## Build

```sh
dotnet build src/AffixSystem/AffixSystem.csproj -c Release
```

Build properties match the other local Path-of-7D2D modlets:

- `Game7D2D` overrides the local game install path.
- `GAME_7D2D` can also provide the game install path.
- `InstallToGame=false` disables live install refresh.

## Validation

- Build in Release.
- Launch the game with EasyAntiCheat off.
- Load a world.
- Run `affix spawn rare`.
- Confirm the item appears in the backpack or drops at the player.
- Confirm the item info panel shows rarity, affix tier lines, and boosted stat
  numbers.
- Run `affix currency 2`, hold the affixed item in the toolbelt, then run
  `affix augment`.
- Confirm the currency count decreases and the Affixes tab shows one additional
  affix.
- Spawn another affixed item, hold it in the toolbelt, and use `Affix Augment`
  from inventory.
- Run `affix list held` and `affix validate gunHandgunT1Pistol 6`.
- Confirm gun-only affixes do not appear as legal melee weapon rolls.
- Run `affix validate meleeToolPickT1IronPickaxe 6` and confirm only tool-legal
  affixes appear.
- Run `affix spawn rare meleeToolPickT1IronPickaxe 6`, hold it, and confirm the
  Affixes tab renders tool affixes and boosted tool stat numbers.
- Run `affix currency 2`, hold the affixed tool, then use `Affix Augment` from
  inventory.
- Run `affix validate armorPrimitiveHelmet 6` and `affix validate armorMinerOutfit 6`
  and confirm only slot-legal armor affixes appear.
- Run `affix spawn rare armorPrimitiveHelmet 6`, inspect it, and confirm the
  Affixes tab renders armor affixes and boosted armor stat numbers.
- Run `affix currency 2`, hold the affixed armor item, then use `Affix Augment`
  from inventory.
- Run `affix debug loot on`, open a never-opened loot container, and confirm
  supported generated weapons, tools, and armor roll Magic or Rare affixes.
