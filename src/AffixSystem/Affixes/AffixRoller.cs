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
            List<AffixDefinition> legal = GetLegalAffixes(itemValue, null);

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
            List<AffixDefinition> legal = GetLegalAffixes(itemValue, existingAffixes);
            if (legal.Count == 0)
            {
                return false;
            }

            AffixDefinition definition = legal[random.Next(legal.Count)];
            int tier = random.Next(1, maxTier + 1);
            affix = new AffixInstance(definition.Id, tier, definition.GetStatValueForTier(tier));
            return true;
        }

        private static List<AffixDefinition> GetLegalAffixes(ItemValue itemValue, IReadOnlyList<AffixInstance> existingAffixes)
        {
            var legal = new List<AffixDefinition>();

            for (int i = 0; i < AffixCatalog.All.Count; i++)
            {
                AffixDefinition definition = AffixCatalog.All[i];
                if (!definition.IsAllowedOn(itemValue.ItemClass))
                {
                    continue;
                }

                if (HasExistingAffix(existingAffixes, definition.Id))
                {
                    continue;
                }

                legal.Add(definition);
            }

            return legal;
        }

        private static bool HasExistingAffix(IReadOnlyList<AffixInstance> existingAffixes, string definitionId)
        {
            if (existingAffixes == null)
            {
                return false;
            }

            for (int i = 0; i < existingAffixes.Count; i++)
            {
                if (existingAffixes[i].DefinitionId == definitionId)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
