using System.Collections.Generic;
using System.Text;

namespace AffixSystem.Affixes
{
    internal static class AffixDisplay
    {
        private const string MagicColor = "6FA8FF";
        private const string RareColor = "FFD166";
        private const string MutedColor = "B8C1CC";
        private const string ValueColor = "8EF6D2";
        private static readonly string[] TierColors =
        {
            "B8C1CC",
            "72D572",
            "5DB7FF",
            "C77DFF",
            "FFB84D",
            "FF5D73"
        };
        private static readonly Dictionary<string, string> RareSuffixes = BuildRareSuffixes();

        public static string BuildItemName(string baseName, AffixItemState state)
        {
            string color = state.Rarity == AffixRarity.Rare ? RareColor : MagicColor;
            string rarity = state.Rarity == AffixRarity.Rare ? "Rare" : "Magic";

            if (state.Rarity == AffixRarity.Magic && state.Affixes.Count > 0 &&
                AffixCatalog.TryGet(state.Affixes[0].DefinitionId, out AffixDefinition first))
            {
                return $"[{color}]{rarity}[-] {first.DisplayName} {baseName}";
            }

            if (state.Rarity == AffixRarity.Rare)
            {
                string prefix = GetAffixDisplayName(state, 0);
                string suffix = GetRareSuffix(state, 1);

                if (!string.IsNullOrEmpty(prefix) && !string.IsNullOrEmpty(suffix))
                {
                    return $"[{color}]{rarity}[-] {prefix} {baseName} of {suffix}";
                }

                if (!string.IsNullOrEmpty(prefix))
                {
                    return $"[{color}]{rarity}[-] {prefix} {baseName}";
                }
            }

            return $"[{color}]{rarity}[-] {baseName}";
        }

        public static string BuildAffixHeader(AffixItemState state)
        {
            string color = state.Rarity == AffixRarity.Rare ? RareColor : MagicColor;
            string rarity = state.Rarity == AffixRarity.Rare ? "Rare" : "Magic";

            return "[" + color + "]" + rarity + " Affixes[-] [" + MutedColor + "](" + state.Affixes.Count + ")[-]";
        }

        public static string BuildAffixTabText(AffixItemState state)
        {
            var builder = new StringBuilder();
            builder.Append(BuildAffixHeader(state)).Append("\n\n");
            AppendAffixRows(builder, state);

            return builder.ToString().TrimEnd();
        }

        private static void AppendAffixRows(StringBuilder builder, AffixItemState state)
        {
            for (int i = 0; i < state.Affixes.Count; i++)
            {
                AffixInstance affix = state.Affixes[i];
                if (!AffixCatalog.TryGet(affix.DefinitionId, out AffixDefinition definition))
                {
                    continue;
                }

                builder
                    .Append('[').Append(GetTierColor(affix.Tier)).Append("]T").Append(affix.Tier).Append("[-] ")
                    .Append("[FFFFFF]").Append(definition.DisplayName).Append("[-]: ")
                    .Append("[").Append(ValueColor).Append(']')
                    .Append(definition.FormatValue(affix.StatValue))
                    .Append("[-]\n");
            }
        }

        public static string BuildSummary(AffixItemState state)
        {
            var builder = new StringBuilder();
            builder.Append(state.Rarity).Append(" item rolled ");
            builder.Append(state.Affixes.Count).Append(" affix(es): ");

            for (int i = 0; i < state.Affixes.Count; i++)
            {
                AffixInstance affix = state.Affixes[i];
                if (!AffixCatalog.TryGet(affix.DefinitionId, out AffixDefinition definition))
                {
                    continue;
                }

                if (i > 0)
                {
                    builder.Append("; ");
                }

                builder.Append(definition.DisplayName)
                    .Append(" T")
                    .Append(affix.Tier)
                    .Append(" (")
                    .Append(definition.FormatValue(affix.StatValue))
                    .Append(')');
            }

            return builder.ToString();
        }

        private static string GetTierColor(int tier)
        {
            int index = tier - 1;
            if (index < 0)
            {
                index = 0;
            }
            else if (index >= TierColors.Length)
            {
                index = TierColors.Length - 1;
            }

            return TierColors[index];
        }

        private static string GetAffixDisplayName(AffixItemState state, int index)
        {
            if (state == null || index < 0 || index >= state.Affixes.Count)
            {
                return null;
            }

            return AffixCatalog.TryGet(state.Affixes[index].DefinitionId, out AffixDefinition definition)
                ? definition.DisplayName
                : null;
        }

        private static string GetRareSuffix(AffixItemState state, int startIndex)
        {
            if (state == null)
            {
                return null;
            }

            for (int i = startIndex; i < state.Affixes.Count; i++)
            {
                string id = state.Affixes[i].DefinitionId;
                if (RareSuffixes.TryGetValue(id, out string suffix))
                {
                    return suffix;
                }

                if (AffixCatalog.TryGet(id, out AffixDefinition definition))
                {
                    return definition.DisplayName;
                }
            }

            return null;
        }

        private static Dictionary<string, string> BuildRareSuffixes()
        {
            return new Dictionary<string, string>
            {
                { "sharpened", "Slaughter" },
                { "crusher", "Ruin" },
                { "reinforced", "Endurance" },
                { "expanded", "Capacity" },
                { "rapid", "Haste" },
                { "farshot", "Distance" },
                { "ranging", "Reach" },
                { "ballistic", "Velocity" },
                { "quickdraw", "Reloading" },
                { "balanced", "Control" },
                { "executioner", "Execution" },
                { "frenzied", "Fury" },
                { "efficient", "Efficiency" },
                { "quarrying", "Quarrying" },
                { "bountiful", "Plenty" },
                { "braced", "Fortitude" },
                { "workhorse", "Industry" },
                { "gravebreaker", "Gravebreaking" },
                { "vital", "Vigor" },
                { "enduring", "Stamina" },
                { "quickstep", "Pace" },
                { "silent", "Silence" },
                { "insulated", "Shelter" },
                { "hardy", "Resilience" },
                { "packrat", "Burden" },
                { "specialist", "Focus" },
                { "wastelandHardened", "Survival" }
            };
        }
    }
}
