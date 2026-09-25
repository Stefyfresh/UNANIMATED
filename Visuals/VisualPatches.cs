using HarmonyLib;
using Overworld;

namespace UNANIMATED.Visuals
{
    // Stop the DayNightSystem from updating
    [HarmonyPatch(typeof(DayNightSystem))]
    [HarmonyPatch("DayTimeChanged")]
    internal class DayNightSystemPatch
    {
        static bool Prefix(ref bool __result)
        {
            if (UNANIMATED.effectsEnabled)
            {
                __result = false;
                return false;
            }
            else return true;
        }
    }
}