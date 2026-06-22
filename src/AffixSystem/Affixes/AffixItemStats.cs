using System.Collections.Generic;
using System.Linq;

namespace AffixSystem.Affixes
{
    internal static class AffixItemStats
    {
        public static void Apply(ItemValue itemValue, AffixItemState state)
        {
            var totals = new Dictionary<PassiveEffects, int>();

            for (int i = 0; i < state.Affixes.Count; i++)
            {
                AffixInstance affix = state.Affixes[i];
                if (!AffixCatalog.TryGet(affix.DefinitionId, out AffixDefinition definition))
                {
                    continue;
                }

                if (!totals.ContainsKey(definition.PassiveEffect))
                {
                    totals[definition.PassiveEffect] = 0;
                }

                totals[definition.PassiveEffect] += affix.StatValue;
            }

            if (totals.Count == 0)
            {
                itemValue.ClearStats();
                return;
            }

            KeyValuePair<PassiveEffects, int>[] ordered = totals
                .OrderBy(pair => pair.Key)
                .ToArray();

            itemValue.InitStats(ordered.Length);
            for (int i = 0; i < ordered.Length; i++)
            {
                itemValue.SetStat(i, ordered[i].Key, 0, ordered[i].Value);
            }
        }
    }
}

