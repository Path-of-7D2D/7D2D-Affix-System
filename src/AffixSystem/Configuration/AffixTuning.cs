using System;
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

        public static int MagicAffixCap { get; private set; } = 2;

        public static int RareAffixCap { get; private set; } = 4;

        public static string AugmentItemName { get; private set; } = DefaultAugmentItemName;

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
                MagicAffixCap = ReadInt(root, "affixCaps/magic", MagicAffixCap, 1, 6);
                RareAffixCap = ReadInt(root, "affixCaps/rare", RareAffixCap, 1, 6);
                AugmentItemName = ReadString(root, "currency/augmentItemName", AugmentItemName);

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
            int magicWeight = Math.Max(0, MagicLootWeight);
            int rareWeight = Math.Max(0, RareLootWeight);
            int total = magicWeight + rareWeight;
            if (total <= 0)
            {
                return AffixRarity.Magic;
            }

            int roll = random.Next(total);
            return roll < rareWeight ? AffixRarity.Rare : AffixRarity.Magic;
        }

        public static int GetAffixCap(AffixRarity rarity)
        {
            return rarity == AffixRarity.Rare ? RareAffixCap : MagicAffixCap;
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
            MagicAffixCap = 2;
            RareAffixCap = 4;
            AugmentItemName = DefaultAugmentItemName;
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
    }
}
