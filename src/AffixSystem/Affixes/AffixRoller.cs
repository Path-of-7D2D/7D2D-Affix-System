using System;
using System.Collections.Generic;

namespace AffixSystem.Affixes
{
    internal static class AffixRoller
    {
        public static AffixItemState Roll(ItemValue itemValue, AffixRarity rarity, Random random)
        {
            int affixCount = rarity == AffixRarity.Rare ? 4 : 2;
            return Roll(itemValue, rarity, random, affixCount);
        }

        public static AffixItemState Roll(ItemValue itemValue, AffixRarity rarity, Random random, int affixCount)
        {
            affixCount = Math.Max(1, affixCount);
            int maxTier = Math.Max(1, Math.Min(6, (int)itemValue.Quality));
            List<AffixDefinition> legal = AffixCatalog.GetLegalAffixes(itemValue);

            var affixes = new List<AffixInstance>();
            while (affixes.Count < affixCount && legal.Count > 0)
            {
                int index = random.Next(legal.Count);
                AffixDefinition definition = legal[index];
                legal.RemoveAt(index);

                int tier = random.Next(1, maxTier + 1);
                affixes.Add(new AffixInstance(definition.Id, tier, definition.GetStatValueForTier(tier)));
            }

            return new AffixItemState(rarity, affixes);
        }

        public static bool TryRollAdditional(ItemValue itemValue, IReadOnlyList<AffixInstance> existingAffixes, Random random, out AffixInstance affix)
        {
            affix = null;
            int maxTier = Math.Max(1, Math.Min(6, (int)itemValue.Quality));
            List<AffixDefinition> legal = AffixCatalog.GetLegalAffixes(itemValue, existingAffixes);
            if (legal.Count == 0)
            {
                return false;
            }

            AffixDefinition definition = legal[random.Next(legal.Count)];
            int tier = random.Next(1, maxTier + 1);
            affix = new AffixInstance(definition.Id, tier, definition.GetStatValueForTier(tier));
            return true;
        }

    }
}
