using System;
using System.Collections.Generic;
using System.Globalization;

namespace AffixSystem.Affixes
{
    internal sealed class AffixItemState
    {
        public const string MetadataKey = "p7d2d.affixes.v1";

        public AffixItemState(AffixRarity rarity, IReadOnlyList<AffixInstance> affixes)
            : this(rarity, affixes, null)
        {
        }

        public AffixItemState(AffixRarity rarity, IReadOnlyList<AffixInstance> affixes, string origin)
        {
            Rarity = rarity;
            Affixes = affixes;
            Origin = string.IsNullOrEmpty(origin) ? null : origin;
        }

        public AffixRarity Rarity { get; }

        public IReadOnlyList<AffixInstance> Affixes { get; }

        public string Origin { get; }

        public bool IsDisplayable => Rarity == AffixRarity.Magic || Rarity == AffixRarity.Rare;

        public static bool TryRead(ItemValue itemValue, out AffixItemState state)
        {
            state = null;
            if (itemValue == null || !itemValue.TryGetMetadata(MetadataKey, out string raw) || string.IsNullOrEmpty(raw))
            {
                return false;
            }

            string[] parts = raw.Split('|');
            if (!TryReadParts(parts, out AffixRarity rarity, out string origin, out string affixPart))
            {
                return false;
            }

            var affixes = new List<AffixInstance>();
            if (!string.IsNullOrEmpty(affixPart))
            {
                string[] entries = affixPart.Split(';');
                for (int i = 0; i < entries.Length; i++)
                {
                    string[] entry = entries[i].Split(',');
                    if (entry.Length != 3)
                    {
                        continue;
                    }

                    if (!int.TryParse(entry[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int tier))
                    {
                        continue;
                    }

                    if (!int.TryParse(entry[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int statValue))
                    {
                        continue;
                    }

                    if (!AffixCatalog.TryGet(entry[0], out _))
                    {
                        continue;
                    }

                    affixes.Add(new AffixInstance(entry[0], tier, statValue));
                }
            }

            if (affixes.Count == 0)
            {
                return false;
            }

            state = new AffixItemState(rarity, affixes, origin);
            return true;
        }

        public static bool TryReadDisplayable(ItemValue itemValue, out AffixItemState state)
        {
            if (!TryRead(itemValue, out state))
            {
                return false;
            }

            return state.IsDisplayable;
        }

        public void WriteTo(ItemValue itemValue)
        {
            itemValue.SetMetadata(MetadataKey, Serialize());
            AffixItemStats.Apply(itemValue, this);
        }

        private string Serialize()
        {
            var entries = new List<string>();
            for (int i = 0; i < Affixes.Count; i++)
            {
                AffixInstance affix = Affixes[i];
                entries.Add(
                    affix.DefinitionId + "," +
                    affix.Tier.ToString(CultureInfo.InvariantCulture) + "," +
                    affix.StatValue.ToString(CultureInfo.InvariantCulture));
            }

            return "2|" + Rarity + "|" + SanitizeMetadataValue(Origin) + "|" + string.Join(";", entries.ToArray());
        }

        private static bool TryReadParts(string[] parts, out AffixRarity rarity, out string origin, out string affixPart)
        {
            rarity = AffixRarity.Magic;
            origin = null;
            affixPart = null;

            if (parts.Length == 3 && parts[0] == "1")
            {
                if (!Enum.TryParse(parts[1], ignoreCase: true, out rarity))
                {
                    return false;
                }

                affixPart = parts[2];
                return true;
            }

            if (parts.Length == 4 && parts[0] == "2")
            {
                if (!Enum.TryParse(parts[1], ignoreCase: true, out rarity))
                {
                    return false;
                }

                origin = string.IsNullOrEmpty(parts[2]) ? null : parts[2];
                affixPart = parts[3];
                return true;
            }

            return false;
        }

        private static string SanitizeMetadataValue(string value)
        {
            return string.IsNullOrEmpty(value)
                ? ""
                : value.Replace("|", "/");
        }
    }
}
