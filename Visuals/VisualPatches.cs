using HarmonyLib;
using Overworld;

namespace UNANIMATED.Visuals
{
    // Stop the DayNightSystem from updating
    [HarmonyPatch(typeof(DayNightSystem))]
    [HarmonyPatch(nameof(DayNightSystem.DayTimeChanged))]
    internal class DayNightSystemDayTimeChangedPatch
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



    [HarmonyPatch(typeof(DayNightSystem))]
    [HarmonyPatch(nameof(DayNightSystem.OnEnable))]
    internal class DayNightSystemOnEnablePatch
    {
        static bool Prefix(ref DayNightSystem __instance)
        {
            if (UNANIMATED.effectsEnabled && VisualController.currentTimeAndWeather != TimeAndWeatherOption.Reset)
            {
                __instance.SetWeatherOverride(VisualController.currentTimeAndWeather.ToString());
                return false;
            }
            else return true;
        }
    }
}