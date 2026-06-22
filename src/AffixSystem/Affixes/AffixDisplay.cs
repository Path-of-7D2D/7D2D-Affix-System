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

        public static string BuildItemName(string baseName, AffixItemState state)
        {
            string color = state.Rarity == AffixRarity.Rare ? RareColor : MagicColor;
            string rarity = state.Rarity == AffixRarity.Rare ? "Rare" : "Magic";

            if (state.Rarity == AffixRarity.Magic && state.Affixes.Count > 0 &&
                AffixCatalog.TryGet(state.Affixes[0].DefinitionId, out AffixDefinition first))
            {
                return $"[{color}]{rarity}[-] {first.DisplayName} {baseName}";
            }

            return $"[{color}]{rarity}[-] {baseName}";
        }

        public static string BuildAffixHeader(AffixItemState state)
        {
            string color = state.Rarity == AffixRarity.Rare ? RareColor : MagicColor;
            string rarity = state.Rarity == AffixRarity.Rare ? "Rare" : "Magic";

            return "[" + color + "]" + rarity + " Affixes[-] [" + MutedColor + "](" + state.Affixes.Count + ")[-]";
        }

        public static string BuildAffixDetails(AffixItemState state)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < state.Affixes.Count; i++)
            {
                AffixInstance affix = state.Affixes[i];
                if (!AffixCatalog.TryGet(affix.DefinitionId, out AffixDefinition definition))
                {
                    continue;
                }

                builder
                    .Append('[').Append(GetTierColor(affix.Tier)).Append("]T").Append(affix.Tier).Append("[-] ")
                    .Append("[FFFFFF]").Append(definition.DisplayName).Append("[-]  ")
                    .Append("[").Append(ValueColor).Append(']')
                    .Append(definition.FormatValue(affix.StatValue))
                    .Append("[-]\n");
            }

            return builder.ToString().TrimEnd();
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
    }
}
