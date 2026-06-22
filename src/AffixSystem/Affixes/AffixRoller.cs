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
            var legal = new List<AffixDefinition>();

            for (int i = 0; i < AffixCatalog.All.Count; i++)
            {
                AffixDefinition definition = AffixCatalog.All[i];
                if (definition.IsAllowedOn(itemValue.ItemClass))
                {
                    legal.Add(definition);
                }
            }

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
    }
}
