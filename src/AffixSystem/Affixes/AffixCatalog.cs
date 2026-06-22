using System.Collections.Generic;

namespace AffixSystem.Affixes
{
    internal static class AffixCatalog
    {
        private static readonly AffixDefinition[] definitions =
        {
            new AffixDefinition("sharpened", "Sharpened", PassiveEffects.EntityDamage, "entity damage", Values(10, 20, 30, 40, 50, 60), "weapon"),
            new AffixDefinition("crusher", "Crusher", PassiveEffects.BlockDamage, "block damage", Values(10, 20, 30, 40, 50, 60), "weapon"),
            new AffixDefinition("reinforced", "Reinforced", PassiveEffects.DegradationMax, "durability", Values(20, 40, 60, 80, 100, 120), "weapon"),
            new AffixDefinition("expanded", "Expanded", PassiveEffects.MagazineSize, "magazine size", Values(10, 20, 30, 40, 50, 60), "weapon"),
            new AffixDefinition("rapid", "Rapid", PassiveEffects.RoundsPerMinute, "rounds per minute", Values(10, 20, 30, 40, 50, 60), "weapon"),
            new AffixDefinition("farshot", "Farshot", PassiveEffects.DamageFalloffRange, "damage falloff range", Values(10, 20, 30, 40, 50, 60), "weapon"),
            new AffixDefinition("ranging", "Ranging", PassiveEffects.MaxRange, "max range", Values(10, 20, 30, 40, 50, 60), "weapon"),
            new AffixDefinition("ballistic", "Ballistic", PassiveEffects.ProjectileVelocity, "projectile velocity", Values(10, 20, 30, 40, 50, 60), "weapon"),
            new AffixDefinition("quickdraw", "Quickdraw", PassiveEffects.ReloadSpeedMultiplier, "reload speed", Values(10, 18, 26, 34, 42, 50), "weapon"),
            new AffixDefinition("balanced", "Balanced", PassiveEffects.WeaponHandling, "weapon handling", Values(10, 20, 30, 40, 50, 60), "weapon"),
            new AffixDefinition("executioner", "Executioner", PassiveEffects.HeadshotDamageModifier, "headshot damage", Values(10, 20, 30, 40, 50, 60), "weapon"),
            new AffixDefinition("frenzied", "Frenzied", PassiveEffects.AttacksPerMinute, "attacks per minute", Values(10, 20, 30, 40, 50, 60), "weapon"),
            new AffixDefinition("efficient", "Efficient", PassiveEffects.StaminaLoss, "stamina cost", Values(-10, -20, -30, -40, -50, -60), "weapon")
        };

        private static readonly Dictionary<string, AffixDefinition> byId = BuildById();

        public static IReadOnlyList<AffixDefinition> All => definitions;

        public static bool TryGet(string id, out AffixDefinition definition)
        {
            return byId.TryGetValue(id, out definition);
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
    }
}

