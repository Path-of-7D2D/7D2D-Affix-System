using System;
using System.Runtime.CompilerServices;
using AffixSystem.Affixes;
using HarmonyLib;

namespace AffixSystem.Patches
{
    [HarmonyPatch(typeof(XUiC_ItemInfoWindow), "Init")]
    internal static class ItemInfoWindowInitPatch
    {
        private static void Postfix(XUiC_ItemInfoWindow __instance)
        {
            ItemInfoWindowAffixViewState state = ItemInfoWindowPatches.GetState(__instance);
            state.AffixButton = __instance.GetChildById("affixButton");

            if (state.AffixButton != null && !state.IsHooked)
            {
                XUiC_ItemInfoWindow owner = __instance;
                state.AffixButton.OnPress += (_, _) => ItemInfoWindowPatches.SelectAffixes(owner);
                state.IsHooked = true;
            }
        }
    }

    [HarmonyPatch(typeof(XUiC_ItemInfoWindow), "StatButton_OnPress")]
    internal static class ItemInfoWindowStatButtonPatch
    {
        private static void Postfix(XUiC_ItemInfoWindow __instance)
        {
            ItemInfoWindowPatches.ClearAffixSelection(__instance);
        }
    }

    [HarmonyPatch(typeof(XUiC_ItemInfoWindow), "DescriptionButton_OnPress")]
    internal static class ItemInfoWindowDescriptionButtonPatch
    {
        private static void Postfix(XUiC_ItemInfoWindow __instance)
        {
            ItemInfoWindowPatches.ClearAffixSelection(__instance);
        }
    }

    [HarmonyPatch(typeof(XUiC_ItemInfoWindow), "SetInfo")]
    internal static class ItemInfoWindowSetInfoPatch
    {
        private static void Postfix(XUiC_ItemInfoWindow __instance)
        {
            if (!ItemInfoWindowPatches.HasAffixes(__instance))
            {
                ItemInfoWindowPatches.ClearAffixSelection(__instance);
            }
        }
    }

    [HarmonyPatch(typeof(XUiC_ItemInfoWindow), "GetBindingValueInternal")]
    internal static class ItemInfoWindowBindingPatch
    {
        private static void Postfix(XUiC_ItemInfoWindow __instance, ref string value, string bindingName, ref bool __result)
        {
            if (__instance?.itemStack == null || __instance.itemStack.IsEmpty())
            {
                return;
            }

            if (ItemInfoWindowPatches.TryHandleCustomBinding(__instance, bindingName, ref value, ref __result))
            {
                return;
            }

            if (!__result)
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

            if (ItemInfoWindowPatches.IsShowingAffixes(__instance) &&
                (bindingName.Equals("showstats", StringComparison.OrdinalIgnoreCase) ||
                 bindingName.Equals("showdescription", StringComparison.OrdinalIgnoreCase)))
            {
                value = false.ToString();
            }
        }
    }

    internal static class ItemInfoWindowPatches
    {
        private static readonly ConditionalWeakTable<XUiC_ItemInfoWindow, ItemInfoWindowAffixViewState> states =
            new ConditionalWeakTable<XUiC_ItemInfoWindow, ItemInfoWindowAffixViewState>();

        public static ItemInfoWindowAffixViewState GetState(XUiC_ItemInfoWindow instance)
        {
            return states.GetValue(instance, _ => new ItemInfoWindowAffixViewState());
        }

        public static void SelectAffixes(XUiC_ItemInfoWindow instance)
        {
            if (!HasAffixes(instance))
            {
                ClearAffixSelection(instance);
                return;
            }

            ItemInfoWindowAffixViewState state = GetState(instance);
            state.ShowAffixes = true;

            instance.showStats = false;
            SetSelected(instance.statButton, false);
            SetSelected(instance.descriptionButton, false);
            SetSelected(state.AffixButton, true);
            instance.IsDirty = true;
        }

        public static void ClearAffixSelection(XUiC_ItemInfoWindow instance)
        {
            ItemInfoWindowAffixViewState state = GetState(instance);
            state.ShowAffixes = false;
            SetSelected(state.AffixButton, false);
            instance.IsDirty = true;
        }

        public static bool HasAffixes(XUiC_ItemInfoWindow instance)
        {
            return instance?.itemStack != null &&
                   !instance.itemStack.IsEmpty() &&
                   AffixItemState.TryRead(instance.itemStack.itemValue, out _);
        }

        public static bool IsShowingAffixes(XUiC_ItemInfoWindow instance)
        {
            return GetState(instance).ShowAffixes && HasAffixes(instance);
        }

        public static bool TryHandleCustomBinding(
            XUiC_ItemInfoWindow instance,
            string bindingName,
            ref string value,
            ref bool result)
        {
            if (bindingName.Equals("hasaffixes", StringComparison.OrdinalIgnoreCase))
            {
                value = HasAffixes(instance).ToString();
                result = true;
                return true;
            }

            if (bindingName.Equals("showaffixes", StringComparison.OrdinalIgnoreCase))
            {
                value = IsShowingAffixes(instance).ToString();
                result = true;
                return true;
            }

            if (bindingName.Equals("affixheader", StringComparison.OrdinalIgnoreCase))
            {
                value = AffixItemState.TryRead(instance.itemStack.itemValue, out AffixItemState state)
                    ? AffixDisplay.BuildAffixHeader(state)
                    : string.Empty;
                result = true;
                return true;
            }

            if (bindingName.Equals("affixdescription", StringComparison.OrdinalIgnoreCase))
            {
                value = AffixItemState.TryRead(instance.itemStack.itemValue, out AffixItemState state)
                    ? AffixDisplay.BuildAffixDetails(state)
                    : string.Empty;
                result = true;
                return true;
            }

            return false;
        }

        private static void SetSelected(XUiController controller, bool selected)
        {
            if (controller?.ViewComponent is XUiV_Button button)
            {
                button.Selected = selected;
            }
        }
    }

    internal sealed class ItemInfoWindowAffixViewState
    {
        public XUiController AffixButton { get; set; }

        public bool IsHooked { get; set; }

        public bool ShowAffixes { get; set; }
    }
}
