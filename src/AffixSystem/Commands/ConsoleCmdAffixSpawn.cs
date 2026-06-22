using System;
using System.Collections.Generic;
using AffixSystem.Affixes;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

namespace AffixSystem.Commands
{
    [Preserve]
    public class ConsoleCmdAffixSpawn : ConsoleCmdAbstract
    {
        private const string DefaultWeapon = "gunHandgunT1Pistol";

        public override bool IsExecuteOnClient => true;

        public override DeviceFlag AllowedDeviceTypesClient =>
            DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX;

        public override string[] getCommands()
        {
            return new[] { "affixspawn", "affixgive" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            if (GameManager.IsDedicatedServer)
            {
                Output("affixspawn must be executed on a client.");
                return;
            }

            if (_params.Count < 1 || !TryParseRarity(_params[0], out AffixRarity rarity))
            {
                Output(getDescription());
                return;
            }

            string itemName = _params.Count > 1 ? _params[1] : DefaultWeapon;
            int quality = ParseQuality(_params.Count > 2 ? _params[2] : null);
            bool drop = _params.Count > 3 && ConsoleHelper.ParseParamBool(_params[3], _invalidStringsAsFalse: true);

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
            if (!IsWeapon(itemValue))
            {
                Output(itemName + " is not tagged as a weapon.");
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

        public override string getDescription()
        {
            return "usage: affixspawn <magic|rare> [itemName=" + DefaultWeapon + "] [quality=6] [drop=false]";
        }

        public override string getHelp()
        {
            return getDescription() + "\n" +
                "Examples:\n" +
                "  affixspawn magic\n" +
                "  affixspawn rare gunHandgunT1Pistol 6\n" +
                "  affixspawn rare meleeWpnBladeT1HuntingKnife 5\n" +
                "  affixspawn rare gunHandgunT1Pistol 6 true";
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

        private static bool IsWeapon(ItemValue itemValue)
        {
            ItemClass itemClass = itemValue.ItemClass;
            return itemClass != null && itemClass.HasAnyTags(FastTags<TagGroup.Global>.Parse("weapon"));
        }

        private static void Output(string message)
        {
            SingletonMonoBehaviour<SdtdConsole>.Instance.Output("[AffixSystem] " + message);
        }
    }
}
