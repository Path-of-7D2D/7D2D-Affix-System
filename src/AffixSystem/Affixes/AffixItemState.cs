using System;
using System.Collections.Generic;
using System.Globalization;

namespace AffixSystem.Affixes
{
    internal sealed class AffixItemState
    {
        public const string MetadataKey = "p7d2d.affixes.v1";

        public AffixItemState(AffixRarity rarity, IReadOnlyList<AffixInstance> affixes)
        {
            Rarity = rarity;
            Affixes = affixes;
        }

        public AffixRarity Rarity { get; }

        public IReadOnlyList<AffixInstance> Affixes { get; }

        public static bool TryRead(ItemValue itemValue, out AffixItemState state)
        {
            state = null;
            if (itemValue == null || !itemValue.TryGetMetadata(MetadataKey, out string raw) || string.IsNullOrEmpty(raw))
            {
                return false;
            }

            string[] parts = raw.Split('|');
            if (parts.Length != 3 || parts[0] != "1")
            {
                return false;
            }

            if (!Enum.TryParse(parts[1], ignoreCase: true, out AffixRarity rarity))
            {
                return false;
            }

            var affixes = new List<AffixInstance>();
            if (!string.IsNullOrEmpty(parts[2]))
            {
                string[] entries = parts[2].Split(';');
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

            state = new AffixItemState(rarity, affixes);
            return true;
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

            return "1|" + Rarity + "|" + string.Join(";", entries.ToArray());
        }
    }
}

