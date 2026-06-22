using System;
using AffixSystem.Affixes;
using AffixSystem.Config;

namespace AffixSystem.Loot
{
    internal static class AffixLootService
    {
        public static int ApplyToGeneratedLoot(ItemStack[] slots, GameRandom gameRandom, string source)
        {
            if (!AffixTuning.LootRollingEnabled)
            {
                AffixTuning.LogLoot(source + ": loot rolling disabled.");
                return 0;
            }

            if (slots == null || slots.Length == 0)
            {
                return 0;
            }

            var random = CreateRandom(gameRandom);
            int rolled = 0;

            for (int i = 0; i < slots.Length; i++)
            {
                ItemStack stack = slots[i];
                if (stack == null || stack.IsEmpty() || stack.count != 1)
                {
                    continue;
                }

                ItemValue itemValue = stack.itemValue;
                if (!AffixEligibility.IsSupportedBaseItem(itemValue))
                {
                    AffixTuning.LogLoot(source + "[" + i + "]: skipped unsupported item.");
                    continue;
                }

                if (AffixItemState.TryRead(itemValue, out _))
                {
                    AffixTuning.LogLoot(source + "[" + i + "]: skipped already affixed item.");
                    continue;
                }

                AffixRarity rarity = AffixTuning.ChooseLootRarity(random, source);
                int affixCount = AffixTuning.GetNaturalAffixCount(rarity, (int)itemValue.Quality, random);
                AffixItemState state = AffixRoller.Roll(itemValue, rarity, random, affixCount, source);
                if (state.Affixes.Count == 0)
                {
                    AffixTuning.LogLoot(source + "[" + i + "]: no legal affixes for " + GetItemName(itemValue) + ".");
                    continue;
                }

                state.WriteTo(itemValue);
                rolled++;
                AffixTuning.LogLoot(source + "[" + i + "]: rolled " + rarity + " " + GetItemName(itemValue) + " Q" + itemValue.Quality + "; " + AffixTuning.GetLootRarityWeightSummary(source) + ".");
            }

            if (rolled > 0)
            {
                AffixTuning.LogLoot(source + ": rolled affixes on " + rolled + " generated item(s).");
            }

            return rolled;
        }

        private static Random CreateRandom(GameRandom gameRandom)
        {
            int seed = unchecked(Environment.TickCount ^ (int)DateTime.UtcNow.Ticks);
            if (gameRandom != null)
            {
                seed = unchecked(seed ^ gameRandom.RandomRange(int.MaxValue));
            }

            return new Random(seed);
        }

        private static string GetItemName(ItemValue itemValue)
        {
            ItemClass itemClass = itemValue?.ItemClass;
            if (itemClass == null)
            {
                return "unknown";
            }

            return itemClass.GetLocalizedItemName();
        }
    }
}
