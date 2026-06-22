using System.Reflection;
using AffixSystem.Config;
using HarmonyLib;

namespace AffixSystem
{
    public class AffixSystemModApi : IModApi
    {
        public void InitMod(Mod _modInstance)
        {
            const string id = "com.pathof7d2d.affixsystem";
            Log.Out("[AffixSystem] InitMod running...");

            AffixTuning.Load(_modInstance);

            var harmony = new Harmony(id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            var patched = 0;
            foreach (var _ in harmony.GetPatchedMethods())
            {
                patched++;
            }

            Log.Out($"[AffixSystem] Harmony applied {patched} patch target(s).");
        }
    }
}
