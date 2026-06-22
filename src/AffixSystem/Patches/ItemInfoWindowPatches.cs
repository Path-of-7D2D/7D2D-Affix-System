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

            ItemInfoWindowPatches.SyncAffixControls(__instance);
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
            ItemInfoWindowPatches.SyncAffixControls(__instance);
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

            if (!__result)
            {
                return;
            }

            if (!AffixItemState.TryRead(__instance.itemStack.itemValue, out AffixItemState affixState))
            {
                return;
            }

            if (bindingName.Equals("itemname", StringComparison.OrdinalIgnoreCase))
            {
                value = AffixDisplay.BuildItemName(value, affixState);
                return;
            }

            if (bindingName.Equals("itemdescription", StringComparison.OrdinalIgnoreCase) &&
                ItemInfoWindowPatches.IsShowingAffixes(__instance))
            {
                value = AffixDisplay.BuildAffixTabText(affixState);
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
            if (!TryGetDisplayableAffixes(instance, out _))
            {
                ClearAffixSelection(instance);
                return;
            }

            ItemInfoWindowAffixViewState state = GetState(instance);
            state.ShowAffixes = true;

            instance.showStats = false;
            SetSelected(instance.statButton, false);
            SetSelected(instance.descriptionButton, false);
            SyncAffixControls(instance);
            RefreshTabBindings(instance);
        }

        public static void ClearAffixSelection(XUiC_ItemInfoWindow instance)
        {
            ItemInfoWindowAffixViewState state = GetState(instance);
            state.ShowAffixes = false;
            SetSelected(state.AffixButton, false);
            RefreshTabBindings(instance);
        }

        public static void SyncAffixControls(XUiC_ItemInfoWindow instance)
        {
            ItemInfoWindowAffixViewState state = GetState(instance);
            bool hasAffixes = TryGetDisplayableAffixes(instance, out AffixItemState affixState);

            if (!hasAffixes)
            {
                state.ShowAffixes = false;
            }

            SetViewVisible(state.AffixButton, hasAffixes);
            SetSelected(state.AffixButton, hasAffixes && state.ShowAffixes);
        }

        public static bool HasAffixes(XUiC_ItemInfoWindow instance)
        {
            return TryGetDisplayableAffixes(instance, out _);
        }

        public static bool IsShowingAffixes(XUiC_ItemInfoWindow instance)
        {
            return GetState(instance).ShowAffixes && TryGetDisplayableAffixes(instance, out _);
        }

        private static bool TryGetDisplayableAffixes(XUiC_ItemInfoWindow instance, out AffixItemState state)
        {
            if (instance?.itemStack == null || instance.itemStack.IsEmpty())
            {
                state = null;
                return false;
            }

            return AffixEligibility.TryGetDisplayableState(instance.itemStack.itemValue, out state);
        }

        private static void SetViewVisible(XUiController controller, bool visible)
        {
            if (controller?.ViewComponent != null)
            {
                controller.ViewComponent.IsVisible = visible;
            }
        }

        private static void RefreshTabBindings(XUiC_ItemInfoWindow instance)
        {
            if (instance == null)
            {
                return;
            }

            instance.IsDirty = true;
            instance.RefreshBindings();
            SyncAffixControls(instance);
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
