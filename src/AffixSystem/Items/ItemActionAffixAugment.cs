using System;
using AffixSystem.Affixes;
using AffixSystem.Config;

public class ItemActionAffixAugment : ItemAction
{
    public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
    {
        if (_bReleased || _actionData?.invData == null)
        {
            return;
        }

        EntityPlayerLocal player = _actionData.invData.holdingEntity as EntityPlayerLocal ?? GameManager.Instance.World.GetPrimaryPlayer();
        TryUseAugment(player, _actionData.invData.itemStack, null);
    }

    public override bool ExecuteInstantAction(EntityAlive _entityAlive, ItemStack _itemStack, bool _bReleased, XUiC_ItemStack _uiItemStack)
    {
        EntityPlayerLocal player = _entityAlive as EntityPlayerLocal ?? GameManager.Instance.World.GetPrimaryPlayer();
        return TryUseAugment(player, _itemStack, _uiItemStack);
    }

    private static bool TryUseAugment(EntityPlayerLocal player, ItemStack sourceStack, XUiC_ItemStack uiItemStack)
    {
        if (player == null)
        {
            return false;
        }

        if (sourceStack == null || sourceStack.IsEmpty() || sourceStack.count <= 0)
        {
            ShowTooltip(player, "No Affix Augment stack is available.");
            return false;
        }

        ItemStack target = player.inventory.holdingItemStack;
        if (target == null || target.IsEmpty() || player.inventory.holdingCount <= 0)
        {
            ShowTooltip(player, "Hold a Magic or Rare affixed item in the toolbelt first.");
            return false;
        }

        if (IsAugmentCurrency(target.itemValue))
        {
            ShowTooltip(player, "Hold the item you want to augment, then use Affix Augment from inventory.");
            return false;
        }

        ItemValue itemValue = target.itemValue;
        var random = new Random(unchecked(Environment.TickCount ^ itemValue.Seed ^ (int)DateTime.UtcNow.Ticks));
        if (!AffixAugmenter.TryAddAffix(itemValue, random, out AffixItemState newState, out string message))
        {
            ShowTooltip(player, message);
            return false;
        }

        ConsumeSourceStack(sourceStack);
        RefreshUi(player, uiItemStack);
        ShowTooltip(player, message);
        return true;
    }

    private static bool IsAugmentCurrency(ItemValue itemValue)
    {
        if (itemValue == null || itemValue.IsEmpty())
        {
            return false;
        }

        ItemValue lookup = ItemClass.GetItem(AffixTuning.AugmentItemName, _caseInsensitive: true);
        return !lookup.IsEmpty() && itemValue.type == lookup.type;
    }

    private static void ConsumeSourceStack(ItemStack sourceStack)
    {
        sourceStack.count--;
        if (sourceStack.count <= 0)
        {
            sourceStack.count = 0;
            sourceStack.itemValue = ItemValue.None;
        }
    }

    private static void RefreshUi(EntityPlayerLocal player, XUiC_ItemStack uiItemStack)
    {
        uiItemStack?.ForceRefreshItemStack();
        player.inventory.ForceHoldingItemUpdate();
        player.inventory.CallOnToolbeltChangedInternal();
        player.callInventoryChanged();
    }

    private static void ShowTooltip(EntityPlayerLocal player, string message)
    {
        GameManager.ShowTooltip(player, "[AffixSystem] " + message, false, false, 3f);
        Log.Out("[AffixSystem] " + message);
    }
}

public class AffixAugment : ItemActionAffixAugment
{
}
