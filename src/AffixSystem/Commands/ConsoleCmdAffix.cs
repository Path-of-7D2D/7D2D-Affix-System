using System;
using System.Collections.Generic;
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
        private const string DefaultWeapon = "gunHandgunT1Pistol";

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
                InspectHeldItem();
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
            return "usage: affix <spawn|inspect|currency|augment|debug|reload|help>";
        }

        public override string getHelp()
        {
            return getDescription() + "\n" +
                "Subcommands:\n" +
                "  affix spawn <magic|rare> [itemName=" + DefaultWeapon + "] [quality=6] [drop=false]\n" +
                "  affix inspect\n" +
                "  affix currency [count=1]\n" +
                "  affix augment\n" +
                "  affix debug loot <on|off>\n" +
                "  affix reload\n" +
                "Examples:\n" +
                "  affix spawn magic\n" +
                "  affix spawn rare gunHandgunT1Pistol 6\n" +
                "  affix spawn rare meleeWpnBladeT1HuntingKnife 5\n" +
                "  affix spawn rare gunHandgunT1Pistol 6 true\n" +
                "  affix inspect\n" +
                "  affix currency 3\n" +
                "  affix augment\n" +
                "  affix debug loot on";
        }

        private static void Spawn(List<string> parameters)
        {
            if (parameters.Count < 2 || !TryParseRarity(parameters[1], out AffixRarity rarity))
            {
                Output("usage: affix spawn <magic|rare> [itemName=" + DefaultWeapon + "] [quality=6] [drop=false]");
                return;
            }

            string itemName = parameters.Count > 2 ? parameters[2] : DefaultWeapon;
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
            AffixItemState state = AffixRoller.Roll(itemValue, rarity, random, AffixTuning.GetNaturalAffixCount(rarity));
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
                Output("Dropped affixed weapon: " + itemName + " Q" + quality);
            }
            else
            {
                player.bag.AddItem(stack);
                Output("Added affixed weapon to backpack: " + itemName + " Q" + quality);
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
                Output("Hold a Magic or Rare weapon in the toolbelt before running affix augment.");
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

        private static void InspectHeldItem()
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
                return;
            }

            Output(AffixDisplay.BuildSummary(state));
        }

        private static void Debug(List<string> parameters)
        {
            if (parameters.Count < 3 || !parameters[1].Equals("loot", StringComparison.OrdinalIgnoreCase))
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

        private static void Output(string message)
        {
            SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[AffixSystem] " + message);
        }
    }
}
