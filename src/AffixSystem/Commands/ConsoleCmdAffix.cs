using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using AffixSystem.Affixes;
using AffixSystem.Config;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

namespace AffixSystem.Commands
{
    [Preserve]
    public class ConsoleCmdAffix : ConsoleCmdAbstract
    {
        private const string DefaultItem = "gunHandgunT1Pistol";

        public override bool IsExecuteOnClient => true;

        public override DeviceFlag AllowedDeviceTypesClient =>
            DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX;

        public override string[] getCommands()
        {
            return new[] { "affix" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            if (GameManager.IsDedicatedServer)
            {
                Output("affix must be executed on a client.");
                return;
            }

            if (_params.Count < 1)
            {
                Output(getDescription());
                return;
            }

            string subcommand = _params[0];
            if (subcommand.Equals("spawn", StringComparison.OrdinalIgnoreCase))
            {
                Spawn(_params);
                return;
            }

            if (subcommand.Equals("inspect", StringComparison.OrdinalIgnoreCase))
            {
                InspectHeldItem(_params);
                return;
            }

            if (subcommand.Equals("currency", StringComparison.OrdinalIgnoreCase))
            {
                GiveCurrency(_params);
                return;
            }

            if (subcommand.Equals("augment", StringComparison.OrdinalIgnoreCase))
            {
                AugmentHeldItem();
                return;
            }

            if (subcommand.Equals("list", StringComparison.OrdinalIgnoreCase))
            {
                ListAffixes(_params);
                return;
            }

            if (subcommand.Equals("validate", StringComparison.OrdinalIgnoreCase))
            {
                ValidateAffixPool(_params);
                return;
            }

            if (subcommand.Equals("rolltest", StringComparison.OrdinalIgnoreCase))
            {
                RollTest(_params);
                return;
            }

            if (subcommand.Equals("debug", StringComparison.OrdinalIgnoreCase))
            {
                Debug(_params);
                return;
            }

            if (subcommand.Equals("reload", StringComparison.OrdinalIgnoreCase))
            {
                AffixTuning.Reload();
                Output("Reloaded affix tuning config.");
                return;
            }

            if (subcommand.Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                Output(getHelp());
                return;
            }

            Output("Unknown affix subcommand: " + subcommand);
            Output(getDescription());
        }

        public override string getDescription()
        {
            return "usage: affix <spawn|inspect|currency|augment|list|validate|rolltest|debug|reload|help>";
        }

        public override string getHelp()
        {
            return getDescription() + "\n" +
                "Subcommands:\n" +
                "  affix spawn <magic|rare> [itemName=" + DefaultItem + "] [quality=6] [drop=false]\n" +
                "  affix inspect [raw]\n" +
                "  affix currency [count=1]\n" +
                "  affix augment\n" +
                "  affix list [all|held]\n" +
                "  affix validate [itemName] [quality=6]\n" +
                "  affix rolltest <itemName> [quality=6] [samples=1000] [source]\n" +
                "  affix debug loot <on|off>\n" +
                "  affix debug rarity [source]\n" +
                "  affix reload\n" +
                "Examples:\n" +
                "  affix spawn magic\n" +
                "  affix spawn rare gunHandgunT1Pistol 6\n" +
                "  affix spawn rare meleeWpnBladeT1HuntingKnife 5\n" +
                "  affix spawn rare meleeToolPickT1IronPickaxe 6\n" +
                "  affix spawn rare armorPrimitiveHelmet 6\n" +
                "  affix spawn rare gunHandgunT1Pistol 6 true\n" +
                "  affix inspect\n" +
                "  affix inspect raw\n" +
                "  affix currency 3\n" +
                "  affix augment\n" +
                "  affix list held\n" +
                "  affix validate meleeWpnBladeT1HuntingKnife 5\n" +
                "  affix validate meleeToolPickT1IronPickaxe 6\n" +
                "  affix validate armorPrimitiveHelmet 6\n" +
                "  affix rolltest gunHandgunT1Pistol 6 1000 container:hardenedChestT5\n" +
                "  affix debug loot on\n" +
                "  affix debug rarity container:hardenedChestT5";
        }

        private static void Spawn(List<string> parameters)
        {
            if (parameters.Count < 2 || !TryParseRarity(parameters[1], out AffixRarity rarity))
            {
                Output("usage: affix spawn <magic|rare> [itemName=" + DefaultItem + "] [quality=6] [drop=false]");
                return;
            }

            string itemName = parameters.Count > 2 ? parameters[2] : DefaultItem;
            int quality = ParseQuality(parameters.Count > 3 ? parameters[3] : null);
            bool drop = parameters.Count > 4 && ConsoleHelper.ParseParamBool(parameters[4], _invalidStringsAsFalse: true);

            EntityPlayerLocal player = GameManager.Instance.World.GetPrimaryPlayer();
            if (player == null)
            {
                Output("No local player is available.");
                return;
            }

            ItemValue lookup = ItemClass.GetItem(itemName, _caseInsensitive: true);
            if (lookup.IsEmpty())
            {
                Output("Unknown item name: " + itemName);
                return;
            }

            ItemValue itemValue = new ItemValue(lookup.type, quality, quality, _bCreateDefaultModItems: false);
            if (!AffixEligibility.IsSupportedBaseItem(itemValue))
            {
                Output(itemName + " is not a supported affix base item.");
                return;
            }

            var random = new System.Random(unchecked(Environment.TickCount ^ itemValue.Seed ^ (int)DateTime.UtcNow.Ticks));
            int affixCount = AffixTuning.GetNaturalAffixCount(rarity, quality, random);
            AffixItemState state = AffixRoller.Roll(itemValue, rarity, random, affixCount, "command:spawn");
            if (state.Affixes.Count == 0)
            {
                Output("No legal affixes were available for " + itemName + ".");
                return;
            }

            state.WriteTo(itemValue);

            var stack = new ItemStack(itemValue, 1);
            if (drop)
            {
                GameManager.Instance.ItemDropServer(stack, player.position, Vector3.zero);
                Output("Dropped affixed item: " + itemName + " Q" + quality);
            }
            else
            {
                player.bag.AddItem(stack);
                Output("Added affixed item to backpack: " + itemName + " Q" + quality);
            }

            Output(AffixDisplay.BuildSummary(state));
        }

        private static void GiveCurrency(List<string> parameters)
        {
            int count = ParseCount(parameters.Count > 1 ? parameters[1] : null);
            EntityPlayerLocal player = GameManager.Instance.World.GetPrimaryPlayer();
            if (player == null)
            {
                Output("No local player is available.");
                return;
            }

            if (!TryGetAugmentCurrency(out ItemValue currency))
            {
                Output("Unknown augment currency item: " + AffixTuning.AugmentItemName);
                return;
            }

            player.bag.AddItem(new ItemStack(currency, count));
            Output("Added " + count + " " + AffixTuning.AugmentItemName + " to backpack.");
        }

        private static void AugmentHeldItem()
        {
            EntityPlayerLocal player = GameManager.Instance.World.GetPrimaryPlayer();
            if (player == null)
            {
                Output("No local player is available.");
                return;
            }

            ItemStack held = player.inventory.holdingItemStack;
            if (held == null || held.IsEmpty() || player.inventory.holdingCount <= 0)
            {
                Output("Hold a Magic or Rare affixed item in the toolbelt before running affix augment.");
                return;
            }

            if (!TryGetAugmentCurrency(out ItemValue currency))
            {
                Output("Unknown augment currency item: " + AffixTuning.AugmentItemName);
                return;
            }

            if (CountCurrency(player, currency) < 1)
            {
                Output("Requires 1 " + AffixTuning.AugmentItemName + ".");
                return;
            }

            ItemValue itemValue = held.itemValue;
            var random = new System.Random(unchecked(Environment.TickCount ^ itemValue.Seed ^ (int)DateTime.UtcNow.Ticks));
            if (!AffixAugmenter.TryAddAffix(itemValue, random, out AffixItemState newState, out string message))
            {
                Output(message);
                return;
            }

            if (!ConsumeOneCurrency(player, currency))
            {
                Output("Augment succeeded, but currency could not be consumed. Check inventory state.");
            }

            player.inventory.ForceHoldingItemUpdate();
            player.inventory.CallOnToolbeltChangedInternal();

            Output(message);
            Output(AffixDisplay.BuildSummary(newState));
        }

        private static void InspectHeldItem(List<string> parameters)
        {
            EntityPlayerLocal player = GameManager.Instance.World.GetPrimaryPlayer();
            if (player == null)
            {
                Output("No local player is available.");
                return;
            }

            ItemStack held = player.inventory.holdingItemStack;
            if (held == null || held.IsEmpty() || player.inventory.holdingCount <= 0)
            {
                Output("Hold an item in the toolbelt before running affix inspect.");
                return;
            }

            ItemValue itemValue = held.itemValue;
            ItemClass itemClass = itemValue.ItemClass;
            string itemName = itemClass != null ? itemClass.GetLocalizedItemName() : itemValue.type.ToString();

            Output("Held item: " + itemName + " Q" + itemValue.Quality);
            if (!AffixEligibility.TryGetDisplayableState(itemValue, out AffixItemState state))
            {
                Output("No Magic or Rare affixes are stored on this item.");
                if (IsRawInspect(parameters))
                {
                    OutputRawItemMetadata(itemValue, null);
                }

                return;
            }

            Output(AffixDisplay.BuildSummary(state));
            if (IsRawInspect(parameters))
            {
                OutputRawItemMetadata(itemValue, state);
            }
        }

        private static void ListAffixes(List<string> parameters)
        {
            if (parameters.Count > 1 && parameters[1].Equals("held", StringComparison.OrdinalIgnoreCase))
            {
                ValidateAffixPool(new List<string> { "validate" });
                return;
            }

            Output("Affix catalog (" + AffixCatalog.All.Count + "):");
            for (int i = 0; i < AffixCatalog.All.Count; i++)
            {
                AffixDefinition definition = AffixCatalog.All[i];
                Output(definition.Id + " - " + definition.DisplayName + " (" + definition.StatLabel + ", family " + definition.Family + "), allowed: " + definition.RequirementSummary);
            }
        }

        private static void ValidateAffixPool(List<string> parameters)
        {
            if (!TryGetValidationItem(parameters, out ItemValue itemValue, out string itemName))
            {
                return;
            }

            Output("Validating: " + itemName + " Q" + itemValue.Quality);
            if (!AffixEligibility.IsSupportedBaseItem(itemValue))
            {
                Output("Unsupported affix base. Current scope supports quality weapons, tools, and armor.");
                return;
            }

            AffixItemState existingState = null;
            AffixItemState.TryReadDisplayable(itemValue, out existingState);

            if (existingState != null)
            {
                int cap = AffixTuning.GetAffixCap(existingState.Rarity);
                Output("Stored rarity: " + existingState.Rarity + ", affixes: " + existingState.Affixes.Count + "/" + cap);
                Output(AffixDisplay.BuildSummary(existingState));
            }
            else
            {
                Output("No stored Magic/Rare affix state on this item.");
                Output("Natural counts for Q" + itemValue.Quality + ": Magic " +
                    AffixTuning.GetNaturalAffixCountSummary(AffixRarity.Magic, (int)itemValue.Quality) +
                    ", Rare " +
                    AffixTuning.GetNaturalAffixCountSummary(AffixRarity.Rare, (int)itemValue.Quality));
            }

            IReadOnlyList<AffixInstance> existingAffixes = existingState?.Affixes;
            List<AffixDefinition> legal = AffixCatalog.GetLegalAffixes(itemValue, existingAffixes);
            Output("Legal new affixes (" + legal.Count + "): " + BuildAffixDefinitionList(legal));
        }

        private static void RollTest(List<string> parameters)
        {
            if (parameters.Count < 2)
            {
                Output("usage: affix rolltest <itemName> [quality=6] [samples=1000] [source]");
                return;
            }

            string itemName = parameters[1];
            int quality = ParseQuality(parameters.Count > 2 ? parameters[2] : null);
            int samples = ParseSampleCount(parameters.Count > 3 ? parameters[3] : null);
            string source = parameters.Count > 4
                ? string.Join(" ", parameters.GetRange(4, parameters.Count - 4).ToArray())
                : null;

            ItemValue lookup = ItemClass.GetItem(itemName, _caseInsensitive: true);
            if (lookup.IsEmpty())
            {
                Output("Unknown item name: " + itemName);
                return;
            }

            ItemValue testItem = new ItemValue(lookup.type, quality, quality, _bCreateDefaultModItems: false);
            if (!AffixEligibility.IsSupportedBaseItem(testItem))
            {
                Output(itemName + " is not a supported affix base item.");
                return;
            }

            List<AffixDefinition> legal = AffixCatalog.GetLegalAffixes(testItem);
            if (legal.Count == 0)
            {
                Output("No legal affixes are available for " + itemName + " Q" + quality + ".");
                return;
            }

            var random = new System.Random(unchecked(Environment.TickCount ^ lookup.type ^ quality ^ (int)DateTime.UtcNow.Ticks));
            var affixCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            int magic = 0;
            int rare = 0;
            int noAffixes = 0;
            int totalAffixes = 0;
            int totalTier = 0;

            for (int i = 0; i < samples; i++)
            {
                ItemValue itemValue = new ItemValue(lookup.type, quality, quality, _bCreateDefaultModItems: false);
                AffixRarity rarity = AffixTuning.ChooseLootRarity(random, source);
                if (rarity == AffixRarity.Rare)
                {
                    rare++;
                }
                else
                {
                    magic++;
                }

                int affixCount = AffixTuning.GetNaturalAffixCount(rarity, quality, random);
                AffixItemState state = AffixRoller.Roll(itemValue, rarity, random, affixCount);
                if (state.Affixes.Count == 0)
                {
                    noAffixes++;
                    continue;
                }

                totalAffixes += state.Affixes.Count;
                for (int affixIndex = 0; affixIndex < state.Affixes.Count; affixIndex++)
                {
                    AffixInstance affix = state.Affixes[affixIndex];
                    totalTier += affix.Tier;
                    if (!affixCounts.ContainsKey(affix.DefinitionId))
                    {
                        affixCounts[affix.DefinitionId] = 0;
                    }

                    affixCounts[affix.DefinitionId]++;
                }
            }

            Output("Rolltest: " + itemName + " Q" + quality + ", samples " + samples + ", source " + (string.IsNullOrEmpty(source) ? "default loot" : source));
            Output("Weights: " + AffixTuning.GetLootRarityWeightSummary(source));
            Output("Natural counts for Q" + quality + ": Magic " +
                AffixTuning.GetNaturalAffixCountSummary(AffixRarity.Magic, quality) +
                ", Rare " +
                AffixTuning.GetNaturalAffixCountSummary(AffixRarity.Rare, quality));
            Output("Rarity results: Magic " + magic + " (" + FormatPercent(magic, samples) + "), Rare " + rare + " (" + FormatPercent(rare, samples) + ")");
            Output("Average affixes/item: " + FormatAverage(totalAffixes, samples) + ", average tier: " + FormatAverage(totalTier, totalAffixes) + ", no-affix rolls: " + noAffixes);
            Output("Top affixes: " + BuildTopAffixCounts(affixCounts, totalAffixes, 10));
        }

        private static bool TryGetValidationItem(List<string> parameters, out ItemValue itemValue, out string itemName)
        {
            itemValue = null;
            itemName = null;

            if (parameters.Count > 1)
            {
                itemName = parameters[1];
                int quality = ParseQuality(parameters.Count > 2 ? parameters[2] : null);
                ItemValue lookup = ItemClass.GetItem(itemName, _caseInsensitive: true);
                if (lookup.IsEmpty())
                {
                    Output("Unknown item name: " + itemName);
                    return false;
                }

                itemValue = new ItemValue(lookup.type, quality, quality, _bCreateDefaultModItems: false);
                return true;
            }

            EntityPlayerLocal player = GameManager.Instance.World.GetPrimaryPlayer();
            if (player == null)
            {
                Output("No local player is available.");
                return false;
            }

            ItemStack held = player.inventory.holdingItemStack;
            if (held == null || held.IsEmpty() || player.inventory.holdingCount <= 0)
            {
                Output("Hold an item in the toolbelt or pass an item name.");
                return false;
            }

            itemValue = held.itemValue;
            ItemClass itemClass = itemValue.ItemClass;
            itemName = itemClass != null ? itemClass.GetItemName() : itemValue.type.ToString();
            return true;
        }

        private static void Debug(List<string> parameters)
        {
            if (parameters.Count < 2)
            {
                Output("usage: affix debug <loot|rarity>");
                return;
            }

            if (parameters[1].Equals("loot", StringComparison.OrdinalIgnoreCase))
            {
                if (parameters.Count < 3)
                {
                    Output("usage: affix debug loot <on|off>");
                    return;
                }

                if (TryParseToggle(parameters[2], out bool enabled))
                {
                    AffixTuning.SetLootDebugLogging(enabled);
                    Output("Loot debug logging " + (enabled ? "enabled." : "disabled."));
                    return;
                }

                Output("usage: affix debug loot <on|off>");
                return;
            }

            if (parameters[1].Equals("rarity", StringComparison.OrdinalIgnoreCase))
            {
                string source = parameters.Count > 2
                    ? string.Join(" ", parameters.GetRange(2, parameters.Count - 2).ToArray())
                    : null;

                Output("Rarity weights for " + (string.IsNullOrEmpty(source) ? "default loot" : source) + ": " + AffixTuning.GetLootRarityWeightSummary(source));
                Output("High-risk patterns: " + AffixTuning.GetHighRiskSourcePatternSummary());
                return;
            }

            Output("usage: affix debug <loot|rarity>");
        }

        private static bool TryParseToggle(string raw, out bool enabled)
        {
            if (raw.Equals("on", StringComparison.OrdinalIgnoreCase) ||
                raw.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                raw.Equals("1", StringComparison.OrdinalIgnoreCase))
            {
                enabled = true;
                return true;
            }

            if (raw.Equals("off", StringComparison.OrdinalIgnoreCase) ||
                raw.Equals("false", StringComparison.OrdinalIgnoreCase) ||
                raw.Equals("0", StringComparison.OrdinalIgnoreCase))
            {
                enabled = false;
                return true;
            }

            enabled = false;
            return false;
        }

        private static bool IsRawInspect(List<string> parameters)
        {
            return parameters.Count > 1 &&
                (parameters[1].Equals("raw", StringComparison.OrdinalIgnoreCase) ||
                    parameters[1].Equals("metadata", StringComparison.OrdinalIgnoreCase));
        }

        private static void OutputRawItemMetadata(ItemValue itemValue, AffixItemState state)
        {
            string raw = itemValue.TryGetMetadata(AffixItemState.MetadataKey, out string metadata) && !string.IsNullOrEmpty(metadata)
                ? metadata
                : "none";

            Output("Raw item: type " + itemValue.type + ", quality " + itemValue.Quality + ", seed " + itemValue.Seed);
            Output("Raw affix metadata: " + raw);

            if (state == null)
            {
                return;
            }

            int cap = AffixTuning.GetAffixCap(state.Rarity);
            List<AffixDefinition> legal = AffixCatalog.GetLegalAffixes(itemValue, state.Affixes);
            Output("Stored state: " + state.Rarity + ", affixes " + state.Affixes.Count + "/" + cap + ", legal remaining " + legal.Count + ", origin " + (string.IsNullOrEmpty(state.Origin) ? "unknown" : state.Origin));
        }

        private static bool TryParseRarity(string raw, out AffixRarity rarity)
        {
            if (raw.Equals("magic", StringComparison.OrdinalIgnoreCase))
            {
                rarity = AffixRarity.Magic;
                return true;
            }

            if (raw.Equals("rare", StringComparison.OrdinalIgnoreCase))
            {
                rarity = AffixRarity.Rare;
                return true;
            }

            rarity = AffixRarity.Magic;
            return false;
        }

        private static int ParseQuality(string raw)
        {
            if (!int.TryParse(raw, out int quality))
            {
                return 6;
            }

            if (quality < 1)
            {
                return 1;
            }

            if (quality > 6)
            {
                return 6;
            }

            return quality;
        }

        private static int ParseCount(string raw)
        {
            if (!int.TryParse(raw, out int count))
            {
                return 1;
            }

            if (count < 1)
            {
                return 1;
            }

            if (count > 500)
            {
                return 500;
            }

            return count;
        }

        private static int ParseSampleCount(string raw)
        {
            if (!int.TryParse(raw, out int count))
            {
                return 1000;
            }

            if (count < 1)
            {
                return 1;
            }

            if (count > 10000)
            {
                return 10000;
            }

            return count;
        }

        private static bool TryGetAugmentCurrency(out ItemValue currency)
        {
            ItemValue lookup = ItemClass.GetItem(AffixTuning.AugmentItemName, _caseInsensitive: true);
            if (lookup.IsEmpty())
            {
                currency = null;
                return false;
            }

            currency = new ItemValue(lookup.type, false);
            return true;
        }

        private static int CountCurrency(EntityPlayerLocal player, ItemValue currency)
        {
            int count = player.inventory.GetItemCount(currency, false, -1, -1, false);
            count += player.bag.GetItemCount(currency, -1, -1, false);
            return count;
        }

        private static bool ConsumeOneCurrency(EntityPlayerLocal player, ItemValue currency)
        {
            int before = CountCurrency(player, currency);
            if (before < 1)
            {
                return false;
            }

            player.inventory.DecItem(currency, 1, false, null);
            if (CountCurrency(player, currency) < before)
            {
                return true;
            }

            player.bag.DecItem(currency, 1, false, null);
            return CountCurrency(player, currency) < before;
        }

        private static string BuildAffixDefinitionList(List<AffixDefinition> definitions)
        {
            if (definitions.Count == 0)
            {
                return "none";
            }

            var builder = new StringBuilder();
            for (int i = 0; i < definitions.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(definitions[i].Id);
            }

            return builder.ToString();
        }

        private static string BuildTopAffixCounts(Dictionary<string, int> counts, int totalAffixes, int limit)
        {
            if (counts.Count == 0 || totalAffixes <= 0)
            {
                return "none";
            }

            var sorted = new List<KeyValuePair<string, int>>(counts);
            sorted.Sort((left, right) =>
            {
                int valueCompare = right.Value.CompareTo(left.Value);
                return valueCompare != 0
                    ? valueCompare
                    : string.Compare(left.Key, right.Key, StringComparison.OrdinalIgnoreCase);
            });

            var builder = new StringBuilder();
            int count = Math.Min(limit, sorted.Count);
            for (int i = 0; i < count; i++)
            {
                if (i > 0)
                {
                    builder.Append(", ");
                }

                KeyValuePair<string, int> entry = sorted[i];
                builder.Append(entry.Key);
                builder.Append(" ");
                builder.Append(entry.Value.ToString(CultureInfo.InvariantCulture));
                builder.Append(" (");
                builder.Append(FormatPercent(entry.Value, totalAffixes));
                builder.Append(")");
            }

            return builder.ToString();
        }

        private static string FormatPercent(int value, int total)
        {
            if (total <= 0)
            {
                return "0%";
            }

            return ((value * 100.0) / total).ToString("0.0", CultureInfo.InvariantCulture) + "%";
        }

        private static string FormatAverage(int value, int total)
        {
            if (total <= 0)
            {
                return "0";
            }

            return ((double)value / total).ToString("0.00", CultureInfo.InvariantCulture);
        }

        private static void Output(string message)
        {
            SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[AffixSystem] " + message);
        }
    }
}
