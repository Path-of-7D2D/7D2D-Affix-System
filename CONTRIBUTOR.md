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
- Run `affix currency 2`, hold the affixed weapon in the toolbelt, then run
  `affix augment`.
- Confirm the currency count decreases and the Affixes tab shows one additional
  affix.
- Spawn another affixed weapon, hold it in the toolbelt, and use `Affix Augment`
  from inventory.
- Run `affix list held` and `affix validate gunHandgunT1Pistol 6`.
- Confirm gun-only affixes do not appear as legal melee weapon rolls.
- Run `affix debug loot on`, open a never-opened loot container, and confirm
  supported generated weapons roll Magic or Rare affixes.
