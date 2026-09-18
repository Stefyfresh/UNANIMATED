using System;
using System.Collections.Generic;
using System.Linq;
using Arcade.UI.SongSelect;
using HarmonyLib;
using Rhythm;
using TMPro;
using UNANIMATED.Gameplay;
using UNANIMATED.StageScene;
using UnityEngine;

namespace UNANIMATED
{
    // *Controller awake override to make stage switching work
    [HarmonyPatch(typeof(RhythmController))]
    [HarmonyPatch("Awake")]
    internal class ControllerAwakePrefix
    {
        static bool Prefix()
        {
            if (SceneController.preloadingScenes) return false;
            else return true;
        }
    }

    // [HarmonyPatch(typeof(BeatmapParserEngine))]
    // [HarmonyPatch("ParseLineHitObjects")]
    // internal class ParseLineHitObjectsPatch
    // {
    //     static void Postfix(ref List<HitObjectInfo> hitObjects)
    //     {
    //         HitObjectInfo hitObject = hitObjects.Last();
    //         if (hitObject.IsCommand() && hitObject.laneNumber == 2)
    //         {
    //             // UNANIMATED.Logger.LogInfo("try add command");
    //             try
    //             {
    //                 UNANIMATED.commands.Enqueue(hitObject);
    //             }
    //             catch (Exception ex)
    //             {
    //                 UNANIMATED.Logger.LogError($"Failed to add command! {ex}");
    //             }
    //             // UNANIMATED.Logger.LogInfo("Added command");
    //         }
    //     }
    // }



    [HarmonyPatch(typeof(SystemOptions))]
    [HarmonyPatch("GetScreenShake")]
    internal class GetScreenShakePatch
    {
        static void Postfix(ref bool __result)
        {
            if (UNANIMATED.effectsEnabled)
            {
                if (GameplayController.screenShakeOverride) __result = true;
                else __result = false;
            }
        }
    }



    [HarmonyPatch(typeof(SystemOptions))]
    [HarmonyPatch("GetScreenTilt")]
    internal class GetScreenTiltPatch
    {
        static void Postfix(ref bool __result)
        {
            if (UNANIMATED.effectsEnabled)
            {
                if (GameplayController.screenRotOverride) __result = true;
                else __result = false;
            }
        }
    }



    [HarmonyPatch(typeof(SystemOptions))]
    [HarmonyPatch("GetScreenZoom")]
    internal class GetScreenZoomPatch
    {
        static void Postfix(ref bool __result)
        {
            if (UNANIMATED.effectsEnabled)
            {
                if (GameplayController.screenZoomOverride) __result = true;
                else __result = false;
            }
        }
    }



    [HarmonyPatch(typeof(HighScoreScreenArcade))]
    [HarmonyPatch("OnScoreScreenUpdated")]
    internal class ShowUNANIMATEDOnResultsScreen
    {
        static bool Prefix(ref HighScoreScreenArcade __instance)
        {
            if (UNANIMATED.effectsWereEnabled)
            {
                GameObject scoreScreenModifierGO = UnityEngine.Object.Instantiate(__instance.modifierPrefab, __instance.modifierPrefab.transform.parent);
                scoreScreenModifierGO.SetActive(true);
                scoreScreenModifierGO.GetComponentInChildren<TextMeshProUGUI>().text = "<cspace=0.2em>><uppercase>" + "UNANIMATED";

                UNANIMATED.effectsWereEnabled = false;
            }

            return true;
        }
    }



    [HarmonyPatch(typeof(ArcadeSongDatabase))]
    [HarmonyPatch("Awake")]
    internal class ArcadeSongDatabaseAwakePatch
    {
        static void Postfix()
        {
            // Fix if you quit a chart
            UNANIMATED.effectsWereEnabled = false;
        }
    }
}