using System;
using AffixSystem.Affixes;
using HarmonyLib;

namespace AffixSystem.Patches
{
    [HarmonyPatch(typeof(XUiC_ItemInfoWindow), "GetBindingValueInternal")]
    internal static class ItemInfoWindowPatches
    {
        private static void Postfix(XUiC_ItemInfoWindow __instance, ref string value, string bindingName, ref bool __result)
        {
            if (!__result || __instance?.itemStack == null || __instance.itemStack.IsEmpty())
            {
                return;
            }

            if (!AffixItemState.TryRead(__instance.itemStack.itemValue, out AffixItemState state))
            {
                return;
            }

            if (bindingName.Equals("itemname", StringComparison.OrdinalIgnoreCase))
            {
                value = AffixDisplay.BuildItemName(value, state);
                return;
            }

            if (bindingName.Equals("itemdescription", StringComparison.OrdinalIgnoreCase))
            {
                value = AffixDisplay.AppendDescription(value, state);
            }
        }
    }
}
