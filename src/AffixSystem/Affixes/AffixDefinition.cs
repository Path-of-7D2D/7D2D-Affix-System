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
            : this(id, displayName, passiveEffect.ToString(), passiveEffect, statLabel, tierStatValues, 1, 6, requiredAnyTagExpressions)
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
            : this(id, displayName, family, passiveEffect, statLabel, tierStatValues, 1, 6, requiredAnyTagExpressions)
        {
        }

        public AffixDefinition(
            string id,
            string displayName,
            PassiveEffects passiveEffect,
            string statLabel,
            int[] tierStatValues,
            int minQuality,
            int maxQuality,
            params string[] requiredAnyTagExpressions)
            : this(id, displayName, passiveEffect.ToString(), passiveEffect, statLabel, tierStatValues, minQuality, maxQuality, requiredAnyTagExpressions)
        {
        }

        public AffixDefinition(
            string id,
            string displayName,
            string family,
            PassiveEffects passiveEffect,
            string statLabel,
            int[] tierStatValues,
            int minQuality,
            int maxQuality,
            params string[] requiredAnyTagExpressions)
        {
            Id = id;
            DisplayName = displayName;
            Family = string.IsNullOrEmpty(family) ? passiveEffect.ToString() : family;
            PassiveEffect = passiveEffect;
            StatLabel = statLabel;
            TierStatValues = tierStatValues;
            MinQuality = ClampQuality(minQuality);
            MaxQuality = ClampQuality(maxQuality);
            if (MaxQuality < MinQuality)
            {
                MaxQuality = MinQuality;
            }

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

        public int MinQuality { get; }

        public int MaxQuality { get; }

        public string RequirementSummary
        {
            get
            {
                string tags = requiredAnyTagExpressions.Length == 0
                    ? "any supported item"
                    : string.Join(" | ", requiredAnyTagExpressions);

                if (MinQuality <= 1 && MaxQuality >= 6)
                {
                    return tags;
                }

                string quality = MinQuality == MaxQuality
                    ? "Q" + MinQuality
                    : "Q" + MinQuality + "-Q" + MaxQuality;

                return quality + ", " + tags;
            }
        }

        public bool IsAllowedOn(ItemValue itemValue)
        {
            return itemValue != null &&
                IsQualityAllowed((int)itemValue.Quality) &&
                IsAllowedOn(itemValue.ItemClass);
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

        private bool IsQualityAllowed(int quality)
        {
            quality = ClampQuality(quality);
            return quality >= MinQuality && quality <= MaxQuality;
        }

        private static int ClampQuality(int quality)
        {
            if (quality < 1)
            {
                return 1;
            }

            return quality > 6 ? 6 : quality;
        }
    }
}
