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
        }

        private static void Postfix(LootManager __instance, ITileEntityLootable _tileEntity, bool __state)
        {
            if (!__state || _tileEntity == null)
            {
                return;
            }

            string source = string.IsNullOrEmpty(_tileEntity.lootListName)
                ? "container"
                : "container:" + _tileEntity.lootListName;

            AffixLootService.ApplyToGeneratedLoot(_tileEntity.items, __instance.Random, source);
        }

        private static bool IsFreshGeneratedContainer(ITileEntityLootable tileEntity)
        {
            return tileEntity != null &&
                !tileEntity.bPlayerStorage &&
                !tileEntity.bTouched &&
                tileEntity.IsEmpty();
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
