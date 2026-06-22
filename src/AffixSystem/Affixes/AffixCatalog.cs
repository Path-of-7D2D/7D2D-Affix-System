using System.Collections.Generic;

namespace AffixSystem.Affixes
{
    internal static class AffixCatalog
    {
        private const string AnyWeapon = "weapon";
        private const string MeleeWeapon = "meleeWeapon";
        private const string RangedWeapon = "ranged,gun,bow,launcher";
        private const string GunWeapon = "gun";
        private const string AnyTool = "tool";
        private const string HarvestTool = "harvestingSkill,miningTool,salvageTool";
        private const string MotorTool = "motorTool";

        private static readonly AffixDefinition[] definitions =
        {
            new AffixDefinition("sharpened", "Sharpened", PassiveEffects.EntityDamage, "entity damage", Values(10, 20, 30, 40, 50, 60), AnyWeapon),
            new AffixDefinition("crusher", "Crusher", PassiveEffects.BlockDamage, "block damage", Values(10, 20, 30, 40, 50, 60), MeleeWeapon),
            new AffixDefinition("reinforced", "Reinforced", PassiveEffects.DegradationMax, "durability", Values(20, 40, 60, 80, 100, 120), AnyWeapon),
            new AffixDefinition("expanded", "Expanded", PassiveEffects.MagazineSize, "magazine size", Values(10, 20, 30, 40, 50, 60), GunWeapon),
            new AffixDefinition("rapid", "Rapid", PassiveEffects.RoundsPerMinute, "rounds per minute", Values(10, 20, 30, 40, 50, 60), GunWeapon),
            new AffixDefinition("farshot", "Farshot", PassiveEffects.DamageFalloffRange, "damage falloff range", Values(10, 20, 30, 40, 50, 60), RangedWeapon),
            new AffixDefinition("ranging", "Ranging", PassiveEffects.MaxRange, "max range", Values(10, 20, 30, 40, 50, 60), RangedWeapon),
            new AffixDefinition("ballistic", "Ballistic", PassiveEffects.ProjectileVelocity, "projectile velocity", Values(10, 20, 30, 40, 50, 60), RangedWeapon),
            new AffixDefinition("quickdraw", "Quickdraw", PassiveEffects.ReloadSpeedMultiplier, "reload speed", Values(10, 18, 26, 34, 42, 50), RangedWeapon),
            new AffixDefinition("balanced", "Balanced", PassiveEffects.WeaponHandling, "weapon handling", Values(10, 20, 30, 40, 50, 60), RangedWeapon),
            new AffixDefinition("executioner", "Executioner", PassiveEffects.HeadshotDamageModifier, "headshot damage", Values(10, 20, 30, 40, 50, 60), RangedWeapon),
            new AffixDefinition("frenzied", "Frenzied", PassiveEffects.AttacksPerMinute, "attacks per minute", Values(10, 20, 30, 40, 50, 60), MeleeWeapon),
            new AffixDefinition("efficient", "Efficient", PassiveEffects.StaminaLoss, "stamina cost", Values(-10, -20, -30, -40, -50, -60), MeleeWeapon, AnyTool),
            new AffixDefinition("quarrying", "Quarrying", PassiveEffects.BlockDamage, "block damage", Values(10, 20, 30, 40, 50, 60), HarvestTool),
            new AffixDefinition("bountiful", "Bountiful", PassiveEffects.HarvestCount, "resource harvest", Values(10, 20, 30, 40, 50, 60), HarvestTool),
            new AffixDefinition("braced", "Braced", PassiveEffects.DegradationMax, "durability", Values(20, 40, 60, 80, 100, 120), AnyTool),
            new AffixDefinition("workhorse", "Workhorse", PassiveEffects.BlockDamage, "motor tool block damage", Values(8, 16, 24, 32, 40, 48), MotorTool),
            new AffixDefinition("gravebreaker", "Gravebreaker", PassiveEffects.EntityDamage, "entity damage", Values(4, 8, 12, 16, 20, 24), AnyTool)
        };

        private static readonly Dictionary<string, AffixDefinition> byId = BuildById();

        public static IReadOnlyList<AffixDefinition> All => definitions;

        public static bool TryGet(string id, out AffixDefinition definition)
        {
            return byId.TryGetValue(id, out definition);
        }

        public static List<AffixDefinition> GetLegalAffixes(ItemValue itemValue, IReadOnlyList<AffixInstance> existingAffixes = null)
        {
            var legal = new List<AffixDefinition>();
            ItemClass itemClass = itemValue?.ItemClass;

            for (int i = 0; i < definitions.Length; i++)
            {
                AffixDefinition definition = definitions[i];
                if (!definition.IsAllowedOn(itemClass))
                {
                    continue;
                }

                if (HasExistingAffix(existingAffixes, definition.Id))
                {
                    continue;
                }

                legal.Add(definition);
            }

            return legal;
        }

        private static int[] Values(params int[] values)
        {
            return values;
        }

        private static Dictionary<string, AffixDefinition> BuildById()
        {
            var result = new Dictionary<string, AffixDefinition>();
            for (int i = 0; i < definitions.Length; i++)
            {
                result[definitions[i].Id] = definitions[i];
            }

            return result;
        }

        private static bool HasExistingAffix(IReadOnlyList<AffixInstance> existingAffixes, string definitionId)
        {
            if (existingAffixes == null)
            {
                return false;
            }

            for (int i = 0; i < existingAffixes.Count; i++)
            {
                if (existingAffixes[i].DefinitionId == definitionId)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
