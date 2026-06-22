using System;
using System.Collections.Generic;
using AffixSystem.Config;

namespace AffixSystem.Affixes
{
    internal static class AffixAugmenter
    {
        public static bool TryAddAffix(ItemValue itemValue, Random random, out AffixItemState newState, out string message)
        {
            newState = null;

            if (!AffixEligibility.IsSupportedBaseItem(itemValue))
            {
                message = "Held item is not a supported affix base item.";
                return false;
            }

            if (!AffixEligibility.TryGetDisplayableState(itemValue, out AffixItemState currentState))
            {
                message = "Held item must already be Magic or Rare before it can be augmented.";
                return false;
            }

            int cap = AffixTuning.GetAffixCap(currentState.Rarity);
            if (currentState.Affixes.Count >= cap)
            {
                message = currentState.Rarity + " item already has the maximum " + cap + " affix(es).";
                return false;
            }

            if (!AffixRoller.TryRollAdditional(itemValue, currentState.Affixes, random, out AffixInstance addedAffix))
            {
                message = "No legal new affixes are available for this item.";
                return false;
            }

            var affixes = new List<AffixInstance>(currentState.Affixes);
            affixes.Add(addedAffix);

            newState = new AffixItemState(currentState.Rarity, affixes);
            newState.WriteTo(itemValue);

            message = "Added " + FormatAddedAffix(addedAffix) + " Affixes: " + newState.Affixes.Count + "/" + cap + ".";
            return true;
        }

        private static string FormatAddedAffix(AffixInstance affix)
        {
            if (!AffixCatalog.TryGet(affix.DefinitionId, out AffixDefinition definition))
            {
                return "a new affix";
            }

            return definition.DisplayName + " T" + affix.Tier + " (" + definition.FormatValue(affix.StatValue) + ")";
        }
    }
}
