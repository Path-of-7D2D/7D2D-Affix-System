using System.Text;

namespace AffixSystem.Affixes
{
    internal static class AffixDisplay
    {
        private const string MagicColor = "6FA8FF";
        private const string RareColor = "FFD166";
        private const string MutedColor = "B8C1CC";
        private const string ValueColor = "8EF6D2";

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

        public static string AppendDescription(string baseDescription, AffixItemState state)
        {
            var builder = new StringBuilder(baseDescription ?? string.Empty);
            string color = state.Rarity == AffixRarity.Rare ? RareColor : MagicColor;
            string rarity = state.Rarity == AffixRarity.Rare ? "Rare" : "Magic";

            if (builder.Length > 0)
            {
                builder.Append("\n\n");
            }

            builder.Append('[').Append(color).Append(']').Append(rarity).Append(" Affixes[-]\n");

            for (int i = 0; i < state.Affixes.Count; i++)
            {
                AffixInstance affix = state.Affixes[i];
                if (!AffixCatalog.TryGet(affix.DefinitionId, out AffixDefinition definition))
                {
                    continue;
                }

                builder
                    .Append('[').Append(MutedColor).Append("]T").Append(affix.Tier).Append("[-] ")
                    .Append(definition.DisplayName)
                    .Append(": [").Append(ValueColor).Append(']')
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
    }
}

