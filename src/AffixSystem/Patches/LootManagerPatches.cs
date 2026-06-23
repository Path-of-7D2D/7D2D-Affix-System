using System;
using AffixSystem.Config;
using AffixSystem.Loot;
using HarmonyLib;

namespace AffixSystem.Patches
{
    [HarmonyPatch(typeof(LootManager), "LootContainerOpened")]
    internal static class LootManagerLootContainerOpenedPatch
    {
        private static void Prefix(ITileEntityLootable _tileEntity, out bool __state)
        {
            __state = IsFreshGeneratedContainer(_tileEntity);

            if (__state)
            {
                EnsureDeclaredContainerCapacity(_tileEntity, BuildContainerSource(_tileEntity));
            }
        }

        private static void Postfix(LootManager __instance, ITileEntityLootable _tileEntity, bool __state)
        {
            if (_tileEntity == null)
            {
                return;
            }

            string source = BuildContainerSource(_tileEntity);
            EnsureDeclaredContainerCapacity(_tileEntity, source);

            if (!__state)
            {
                return;
            }

            AffixLootService.ApplyToGeneratedLoot(_tileEntity.items, __instance.Random, source);
        }

        private static bool IsFreshGeneratedContainer(ITileEntityLootable tileEntity)
        {
            return tileEntity != null &&
                !tileEntity.bPlayerStorage &&
                !tileEntity.bTouched &&
                tileEntity.IsEmpty();
        }

        internal static string BuildContainerSource(ITileEntityLootable tileEntity)
        {
            if (tileEntity == null || string.IsNullOrEmpty(tileEntity.lootListName))
            {
                return "container";
            }

            return "container:" + tileEntity.lootListName;
        }

        internal static void EnsureDeclaredContainerCapacity(ITileEntityLootable tileEntity, string source)
        {
            try
            {
                if (tileEntity == null || tileEntity.bPlayerStorage)
                {
                    return;
                }

                Vector2i containerSize = ResolveContainerSize(tileEntity, source);
                int targetSlotCount = containerSize.x * containerSize.y;
                if (containerSize.x <= 0 || containerSize.y <= 0 || targetSlotCount <= 0)
                {
                    AffixTuning.LogLoot(source + ": skipped loot slot capacity check; invalid declared size " + containerSize + ".");
                    return;
                }

                ItemStack[] currentSlots = tileEntity.items;
                int currentSlotCount = currentSlots == null ? 0 : currentSlots.Length;
                int nonEmptySlotCount = CountNonEmptySlots(currentSlots);
                if (currentSlotCount >= targetSlotCount)
                {
                    AffixTuning.LogLoot(source + ": loot slots " + currentSlotCount + "/" + targetSlotCount +
                        " already match resolved " + containerSize.x + "x" + containerSize.y +
                        "; non-empty " + nonEmptySlotCount + ".");
                    return;
                }

                ItemStack[] expandedSlots = ItemStack.CreateArray(targetSlotCount);
                if (currentSlots != null)
                {
                    int copyCount = Math.Min(currentSlots.Length, expandedSlots.Length);
                    for (int i = 0; i < copyCount; i++)
                    {
                        if (currentSlots[i] != null)
                        {
                            expandedSlots[i] = currentSlots[i];
                        }
                    }
                }

                tileEntity.items = expandedSlots;
                AffixTuning.LogLoot(source + ": expanded loot slots " + currentSlotCount + " -> " + expandedSlots.Length +
                    " from resolved " + containerSize.x + "x" + containerSize.y +
                    "; preserved non-empty " + nonEmptySlotCount + ".");
            }
            catch (Exception ex)
            {
                AffixTuning.LogLoot(source + ": failed loot slot capacity check. " + ex.GetType().Name + ": " + ex.Message);
            }
        }

        private static Vector2i ResolveContainerSize(ITileEntityLootable tileEntity, string source)
        {
            Vector2i declaredSize = tileEntity.GetContainerSize();
            Vector2i lootDefinitionSize = GetLootDefinitionSize(tileEntity.lootListName);
            int declaredSlots = GetSlotCount(declaredSize);
            int definitionSlots = GetSlotCount(lootDefinitionSize);

            if (definitionSlots > declaredSlots)
            {
                tileEntity.SetContainerSize(lootDefinitionSize, false);
                AffixTuning.LogLoot(source + ": corrected declared loot size " +
                    declaredSize.x + "x" + declaredSize.y + " -> " +
                    lootDefinitionSize.x + "x" + lootDefinitionSize.y + " from loot.xml.");
                return lootDefinitionSize;
            }

            return declaredSize;
        }

        private static Vector2i GetLootDefinitionSize(string lootListName)
        {
            if (string.IsNullOrEmpty(lootListName))
            {
                return Vector2i.zero;
            }

            LootContainer lootContainer = LootContainer.GetLootContainer(lootListName, false);
            return lootContainer == null ? Vector2i.zero : lootContainer.size;
        }

        private static int GetSlotCount(Vector2i size)
        {
            return size.x <= 0 || size.y <= 0 ? 0 : size.x * size.y;
        }

        private static int CountNonEmptySlots(ItemStack[] slots)
        {
            if (slots == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < slots.Length; i++)
            {
                ItemStack stack = slots[i];
                if (stack != null && !stack.IsEmpty())
                {
                    count++;
                }
            }

            return count;
        }
    }

    [HarmonyPatch(typeof(XUiC_LootWindow), "SetTileEntityChest")]
    internal static class XUiCLootWindowSetTileEntityChestPatch
    {
        private static void Prefix(ITileEntityLootable _te)
        {
            LootManagerLootContainerOpenedPatch.EnsureDeclaredContainerCapacity(
                _te,
                LootManagerLootContainerOpenedPatch.BuildContainerSource(_te));
        }
    }

    [HarmonyPatch(typeof(LootManager), "LootBagOpened")]
    internal static class LootManagerLootBagOpenedPatch
    {
        private static void Prefix(Bag _bag, Entity _bagOwner, out bool __state)
        {
            __state = _bag != null &&
                _bagOwner != null &&
                !_bag.Touched &&
                _bag.IsEmpty();
        }

        private static void Postfix(LootManager __instance, Bag _bag, Entity _bagOwner, bool __state)
        {
            if (!__state || _bag == null)
            {
                return;
            }

            AffixLootService.ApplyToGeneratedLoot(_bag.GetSlots(), __instance.Random, BuildLootBagSource(_bagOwner));
        }

        private static string BuildLootBagSource(Entity bagOwner)
        {
            if (bagOwner == null)
            {
                return "loot-bag";
            }

            string ownerName = bagOwner.name;
            return string.IsNullOrEmpty(ownerName)
                ? "loot-bag"
                : "loot-bag:" + ownerName;
        }
    }
}
