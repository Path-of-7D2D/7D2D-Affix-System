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
- Confirm the item info panel shows a Rare item name with an affix-derived
  prefix/suffix, affix tier lines, and boosted stat numbers.
- Run `affix currency 2`, hold the affixed item in the toolbelt, then run
  `affix augment`.
- Confirm the currency count decreases and the Affixes tab shows one additional
  affix.
- Spawn another affixed item, hold it in the toolbelt, and use `Affix Augment`
  from inventory.
- Run `affix list held` and `affix validate gunHandgunT1Pistol 6`.
- Run `affix validate gunHandgunT1Pistol 1` and `affix validate gunHandgunT1Pistol 6`
  and confirm natural affix count output changes by quality.
- Confirm the Q1 validation output excludes quality-gated advanced affixes such
  as `balanced` and `executioner`, while Q6 includes them.
- Confirm gun-only affixes do not appear as legal melee weapon rolls.
- Run `affix validate gunBowT1WoodenBow 6` and confirm bow/crossbow affixes can
  appear, while gun-only affixes do not.
- Run `affix validate gunBowT1IronCrossbow 6` and confirm crossbow reload
  affixes can appear.
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
- Run `affix list all` and confirm each affix reports a family.
- After natural rolls and augments, run `affix inspect` and confirm the item
  does not contain two affixes from the same family.
- Run `affix inspect raw`, record the raw metadata, move the item between
  toolbelt/backpack/container, drop and pick it back up, then confirm
  `affix inspect raw` reports the same affix metadata.
- After a world reload, hold the same item and confirm `affix inspect raw` still
  reports the same metadata and affix count.
- Run `affix debug loot on`, open a never-opened loot container, and confirm
  supported generated weapons, tools, and armor roll Magic or Rare affixes.
- Run `affix debug rarity container:hardenedChestT5` and confirm it reports a
  high-risk source with increased Rare weight.
- Run `affix debug rarity container:groupSmallWeaponBag` and confirm it reports
  the standard Magic/Rare weights.
- Run `affix rolltest gunHandgunT1Pistol 6 1000 container:groupSmallWeaponBag`
  and `affix rolltest gunHandgunT1Pistol 6 1000 container:hardenedChestT5`;
  confirm the hardened chest sample trends toward a higher Rare rate.
- Open high-value loot such as weapon bags, toolboxes, armor racks,
  reinforced/hardened chests, infestation rewards, or zombie loot bags and
  confirm `Affix Augment` can appear without being sold as normal trader stock.
