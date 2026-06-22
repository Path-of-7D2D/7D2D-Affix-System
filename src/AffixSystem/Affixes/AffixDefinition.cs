using System;

namespace AffixSystem.Affixes
{
    internal sealed class AffixDefinition
    {
        private readonly FastTags<TagGroup.Global>[] requiredAnyTags;
        private readonly string[] requiredAnyTagExpressions;

        public AffixDefinition(
            string id,
            string displayName,
            PassiveEffects passiveEffect,
            string statLabel,
            int[] tierStatValues,
            params string[] requiredAnyTagExpressions)
            : this(id, displayName, passiveEffect.ToString(), passiveEffect, statLabel, tierStatValues, requiredAnyTagExpressions)
        {
        }

        public AffixDefinition(
            string id,
            string displayName,
            string family,
            PassiveEffects passiveEffect,
            string statLabel,
            int[] tierStatValues,
            params string[] requiredAnyTagExpressions)
        {
            Id = id;
            DisplayName = displayName;
            Family = string.IsNullOrEmpty(family) ? passiveEffect.ToString() : family;
            PassiveEffect = passiveEffect;
            StatLabel = statLabel;
            TierStatValues = tierStatValues;
            this.requiredAnyTagExpressions = requiredAnyTagExpressions;
            requiredAnyTags = new FastTags<TagGroup.Global>[requiredAnyTagExpressions.Length];

            for (int i = 0; i < requiredAnyTagExpressions.Length; i++)
            {
                requiredAnyTags[i] = FastTags<TagGroup.Global>.Parse(requiredAnyTagExpressions[i]);
            }
        }

        public string Id { get; }

        public string DisplayName { get; }

        public string Family { get; }

        public PassiveEffects PassiveEffect { get; }

        public string StatLabel { get; }

        public int[] TierStatValues { get; }

        public string RequirementSummary
        {
            get
            {
                return requiredAnyTagExpressions.Length == 0
                    ? "any supported item"
                    : string.Join(" | ", requiredAnyTagExpressions);
            }
        }

        public bool IsAllowedOn(ItemClass itemClass)
        {
            if (!AffixEligibility.IsSupportedBaseItem(itemClass))
            {
                return false;
            }

            if (requiredAnyTags.Length == 0)
            {
                return true;
            }

            for (int i = 0; i < requiredAnyTags.Length; i++)
            {
                if (itemClass.HasAnyTags(requiredAnyTags[i]))
                {
                    return true;
                }
            }

            return false;
        }

        public int GetStatValueForTier(int tier)
        {
            int index = Math.Max(0, Math.Min(TierStatValues.Length - 1, tier - 1));
            return TierStatValues[index];
        }

        public string FormatValue(int statValue)
        {
            float percent = statValue * 0.5f;
            string prefix = percent > 0f ? "+" : "";
            return prefix + percent.ToString("0.#") + "% " + StatLabel;
        }
    }
}
