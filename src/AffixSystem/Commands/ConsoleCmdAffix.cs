using System;
using System.Collections.Generic;
using AffixSystem.Affixes;
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
            return "usage: affix <spawn|inspect|help>";
        }

        public override string getHelp()
        {
            return getDescription() + "\n" +
                "Subcommands:\n" +
                "  affix spawn <magic|rare> [itemName=" + DefaultWeapon + "] [quality=6] [drop=false]\n" +
                "  affix inspect\n" +
                "Examples:\n" +
                "  affix spawn magic\n" +
                "  affix spawn rare gunHandgunT1Pistol 6\n" +
                "  affix spawn rare meleeWpnBladeT1HuntingKnife 5\n" +
                "  affix spawn rare gunHandgunT1Pistol 6 true\n" +
                "  affix inspect";
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
            AffixItemState state = AffixRoller.Roll(itemValue, rarity, random);
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

        private static void Output(string message)
        {
            SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[AffixSystem] " + message);
        }
    }
}
