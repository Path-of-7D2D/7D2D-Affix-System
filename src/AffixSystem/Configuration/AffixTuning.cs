using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using AffixSystem.Affixes;

namespace AffixSystem.Config
{
    internal static class AffixTuning
    {
        private const string DefaultAugmentItemName = "resourceAffixAugment";
        private const string ConfigFileName = "affix_tuning.xml";

        private static string configPath;

        public static bool LootRollingEnabled { get; private set; } = true;

        public static bool LootDebugLogging { get; private set; }

        public static int MagicLootWeight { get; private set; } = 75;

        public static int RareLootWeight { get; private set; } = 25;

        public static int HighRiskRareBonus { get; private set; } = 10;

        public static int MagicNaturalAffixCount { get; private set; } = 2;

        public static int RareNaturalAffixCount { get; private set; } = 4;

        public static int MagicAffixCap { get; private set; } = 3;

        public static int RareAffixCap { get; private set; } = 6;

        public static string AugmentItemName { get; private set; } = DefaultAugmentItemName;

        private static IntRange[] magicNaturalAffixCountsByQuality = BuildDefaultMagicCountRanges();
        private static IntRange[] rareNaturalAffixCountsByQuality = BuildDefaultRareCountRanges();
        private static string[] highRiskSourcePatterns = BuildDefaultHighRiskSourcePatterns();

        public static void Load(Mod modInstance)
        {
            ResetDefaults();
            configPath = ResolveConfigPath(modInstance);

            if (string.IsNullOrEmpty(configPath) || !File.Exists(configPath))
            {
                Log.Out("[AffixSystem] Tuning config not found; using defaults.");
                return;
            }

            try
            {
                var document = new XmlDocument();
                document.Load(configPath);

                XmlNode root = document.SelectSingleNode("/affixTuning");
                if (root == null)
                {
                    Log.Out("[AffixSystem] Tuning config is missing affixTuning root; using defaults.");
                    return;
                }

                LootRollingEnabled = ReadBool(root, "loot/enabled", LootRollingEnabled);
                LootDebugLogging = ReadBool(root, "loot/debugLogging", LootDebugLogging);
                MagicLootWeight = ReadInt(root, "loot/rarityWeights/magic", MagicLootWeight, 0, 100000);
                RareLootWeight = ReadInt(root, "loot/rarityWeights/rare", RareLootWeight, 0, 100000);
                HighRiskRareBonus = ReadInt(root, "loot/rarityWeights/highRiskRareBonus", HighRiskRareBonus, 0, 100000);
                MagicNaturalAffixCount = ReadInt(root, "loot/affixCounts/magic", MagicNaturalAffixCount, 1, 6);
                RareNaturalAffixCount = ReadInt(root, "loot/affixCounts/rare", RareNaturalAffixCount, 1, 6);
                MagicAffixCap = ReadInt(root, "affixCaps/magic", MagicAffixCap, 1, 6);
                RareAffixCap = ReadInt(root, "affixCaps/rare", RareAffixCap, 1, 6);
                AugmentItemName = ReadString(root, "currency/augmentItemName", AugmentItemName);
                ReadHighRiskSourcePatterns(root);

                if (!ReadQualityCountRanges(root))
                {
                    FillCountRangesFromFixedCounts();
                }

                Log.Out("[AffixSystem] Loaded tuning config: " + configPath);
            }
            catch (Exception ex)
            {
                Log.Out("[AffixSystem] Failed to load tuning config; using defaults. " + ex.Message);
            }
        }

        public static void Reload()
        {
            Load(null);
        }

        public static void SetLootDebugLogging(bool enabled)
        {
            LootDebugLogging = enabled;
            Log.Out("[AffixSystem] Loot debug logging " + (enabled ? "enabled." : "disabled."));
        }

        public static AffixRarity ChooseLootRarity(Random random)
        {
            return ChooseLootRarity(random, null);
        }

        public static AffixRarity ChooseLootRarity(Random random, string source)
        {
            int magicWeight = Math.Max(0, MagicLootWeight);
            int rareWeight = Math.Max(0, RareLootWeight);
            ApplyHighRiskRarityBias(source, ref magicWeight, ref rareWeight);

            int total = magicWeight + rareWeight;
            if (total <= 0)
            {
                return AffixRarity.Magic;
            }

            int roll = random.Next(total);
            return roll < rareWeight ? AffixRarity.Rare : AffixRarity.Magic;
        }

        public static string GetLootRarityWeightSummary(string source)
        {
            int magicWeight = Math.Max(0, MagicLootWeight);
            int rareWeight = Math.Max(0, RareLootWeight);
            bool highRisk = IsHighRiskLootSource(source);
            ApplyHighRiskRarityBias(source, ref magicWeight, ref rareWeight);

            return "Magic " + magicWeight.ToString(CultureInfo.InvariantCulture) +
                ", Rare " + rareWeight.ToString(CultureInfo.InvariantCulture) +
                (highRisk ? " (high-risk source)" : " (standard source)");
        }

        public static string GetHighRiskSourcePatternSummary()
        {
            if (highRiskSourcePatterns == null || highRiskSourcePatterns.Length == 0)
            {
                return "none";
            }

            return string.Join(", ", highRiskSourcePatterns);
        }

        public static bool IsHighRiskLootSource(string source)
        {
            if (string.IsNullOrEmpty(source) || highRiskSourcePatterns == null)
            {
                return false;
            }

            for (int i = 0; i < highRiskSourcePatterns.Length; i++)
            {
                string pattern = highRiskSourcePatterns[i];
                if (!string.IsNullOrEmpty(pattern) &&
                    source.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        public static int GetAffixCap(AffixRarity rarity)
        {
            return rarity == AffixRarity.Rare ? RareAffixCap : MagicAffixCap;
        }

        public static int GetNaturalAffixCount(AffixRarity rarity)
        {
            return rarity == AffixRarity.Rare ? RareNaturalAffixCount : MagicNaturalAffixCount;
        }

        public static int GetNaturalAffixCount(AffixRarity rarity, int quality, Random random)
        {
            IntRange range = GetNaturalAffixCountRange(rarity, quality);
            if (range.Min >= range.Max || random == null)
            {
                return range.Min;
            }

            return random.Next(range.Min, range.Max + 1);
        }

        public static string GetNaturalAffixCountSummary(AffixRarity rarity, int quality)
        {
            IntRange range = GetNaturalAffixCountRange(rarity, quality);
            if (range.Min == range.Max)
            {
                return range.Min.ToString(CultureInfo.InvariantCulture);
            }

            return range.Min.ToString(CultureInfo.InvariantCulture) + "-" + range.Max.ToString(CultureInfo.InvariantCulture);
        }

        public static void LogLoot(string message)
        {
            if (LootDebugLogging)
            {
                Log.Out("[AffixSystem] [Loot] " + message);
            }
        }

        private static void ResetDefaults()
        {
            LootRollingEnabled = true;
            LootDebugLogging = false;
            MagicLootWeight = 75;
            RareLootWeight = 25;
            HighRiskRareBonus = 10;
            MagicNaturalAffixCount = 2;
            RareNaturalAffixCount = 4;
            MagicAffixCap = 3;
            RareAffixCap = 6;
            AugmentItemName = DefaultAugmentItemName;
            magicNaturalAffixCountsByQuality = BuildDefaultMagicCountRanges();
            rareNaturalAffixCountsByQuality = BuildDefaultRareCountRanges();
            highRiskSourcePatterns = BuildDefaultHighRiskSourcePatterns();
        }

        private static string ResolveConfigPath(Mod modInstance)
        {
            if (modInstance != null && !string.IsNullOrEmpty(modInstance.Path))
            {
                return Path.Combine(modInstance.Path, "Config", ConfigFileName);
            }

            if (!string.IsNullOrEmpty(configPath))
            {
                return configPath;
            }

            return Path.Combine(Environment.CurrentDirectory, "Mods", "1A-AffixSystem", "Config", ConfigFileName);
        }

        private static bool ReadBool(XmlNode root, string path, bool fallback)
        {
            string raw = ReadString(root, path, null);
            if (string.IsNullOrEmpty(raw))
            {
                return fallback;
            }

            if (bool.TryParse(raw, out bool value))
            {
                return value;
            }

            if (raw == "1")
            {
                return true;
            }

            if (raw == "0")
            {
                return false;
            }

            return fallback;
        }

        private static int ReadInt(XmlNode root, string path, int fallback, int min, int max)
        {
            string raw = ReadString(root, path, null);
            if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
            {
                return fallback;
            }

            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }

        private static string ReadString(XmlNode root, string path, string fallback)
        {
            XmlNode node = root.SelectSingleNode(path);
            if (node == null || string.IsNullOrEmpty(node.InnerText))
            {
                return fallback;
            }

            return node.InnerText.Trim();
        }

        private static void ReadHighRiskSourcePatterns(XmlNode root)
        {
            XmlNodeList nodes = root.SelectNodes("loot/highRiskSources/source");
            if (nodes == null || nodes.Count == 0)
            {
                return;
            }

            var patterns = new List<string>();
            for (int i = 0; i < nodes.Count; i++)
            {
                string pattern = nodes[i]?.InnerText?.Trim();
                if (!string.IsNullOrEmpty(pattern))
                {
                    patterns.Add(pattern);
                }
            }

            if (patterns.Count > 0)
            {
                highRiskSourcePatterns = patterns.ToArray();
            }
        }

        private static bool ReadQualityCountRanges(XmlNode root)
        {
            XmlNodeList nodes = root.SelectNodes("loot/affixCountsByQuality/quality");
            if (nodes == null || nodes.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                if (node?.Attributes == null)
                {
                    continue;
                }

                if (!int.TryParse(node.Attributes["value"]?.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int quality))
                {
                    continue;
                }

                quality = ClampQuality(quality);
                magicNaturalAffixCountsByQuality[quality] = ReadRangeAttribute(node, "magic", magicNaturalAffixCountsByQuality[quality]);
                rareNaturalAffixCountsByQuality[quality] = ReadRangeAttribute(node, "rare", rareNaturalAffixCountsByQuality[quality]);
            }

            return true;
        }

        private static IntRange ReadRangeAttribute(XmlNode node, string attributeName, IntRange fallback)
        {
            string raw = node.Attributes?[attributeName]?.Value;
            if (string.IsNullOrEmpty(raw))
            {
                return fallback;
            }

            string[] parts = raw.Split(',');
            if (!int.TryParse(parts[0].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int min))
            {
                return fallback;
            }

            int max = min;
            if (parts.Length > 1 && !int.TryParse(parts[1].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out max))
            {
                return fallback;
            }

            min = ClampAffixCount(min);
            max = ClampAffixCount(max);
            if (max < min)
            {
                max = min;
            }

            return new IntRange(min, max);
        }

        private static IntRange GetNaturalAffixCountRange(AffixRarity rarity, int quality)
        {
            IntRange[] ranges = rarity == AffixRarity.Rare
                ? rareNaturalAffixCountsByQuality
                : magicNaturalAffixCountsByQuality;

            return ranges[ClampQuality(quality)];
        }

        private static void FillCountRangesFromFixedCounts()
        {
            var magic = new IntRange(MagicNaturalAffixCount, MagicNaturalAffixCount);
            var rare = new IntRange(RareNaturalAffixCount, RareNaturalAffixCount);
            for (int quality = 1; quality <= 6; quality++)
            {
                magicNaturalAffixCountsByQuality[quality] = magic;
                rareNaturalAffixCountsByQuality[quality] = rare;
            }
        }

        private static IntRange[] BuildDefaultMagicCountRanges()
        {
            var ranges = new IntRange[7];
            ranges[1] = new IntRange(1, 1);
            ranges[2] = new IntRange(1, 2);
            ranges[3] = new IntRange(2, 2);
            ranges[4] = new IntRange(2, 2);
            ranges[5] = new IntRange(2, 2);
            ranges[6] = new IntRange(2, 2);
            return ranges;
        }

        private static IntRange[] BuildDefaultRareCountRanges()
        {
            var ranges = new IntRange[7];
            ranges[1] = new IntRange(2, 2);
            ranges[2] = new IntRange(2, 3);
            ranges[3] = new IntRange(3, 3);
            ranges[4] = new IntRange(3, 4);
            ranges[5] = new IntRange(3, 5);
            ranges[6] = new IntRange(3, 6);
            return ranges;
        }

        private static string[] BuildDefaultHighRiskSourcePatterns()
        {
            return new[]
            {
                "reinforcedChestT3",
                "hardenedChest",
                "infestedT4",
                "infestedT5"
            };
        }

        private static void ApplyHighRiskRarityBias(string source, ref int magicWeight, ref int rareWeight)
        {
            if (!IsHighRiskLootSource(source))
            {
                return;
            }

            int bonus = Math.Min(magicWeight, Math.Max(0, HighRiskRareBonus));
            magicWeight -= bonus;
            rareWeight += bonus;
        }

        private static int ClampQuality(int quality)
        {
            if (quality < 1)
            {
                return 1;
            }

            return quality > 6 ? 6 : quality;
        }

        private static int ClampAffixCount(int count)
        {
            if (count < 1)
            {
                return 1;
            }

            return count > 6 ? 6 : count;
        }

        private readonly struct IntRange
        {
            public IntRange(int min, int max)
            {
                Min = min;
                Max = max;
            }

            public int Min { get; }

            public int Max { get; }
        }
    }
}
