using HarmonyLib;
using Rhythm;
using UNANIMATED.CameraControl;
using UnityEngine;

namespace UNANIMATED.UI
{

    [HarmonyPatch(typeof(RhythmCanvasCamFollower))]
    [HarmonyPatch("Update")]
    internal class CameraFixedUpdatePatch
    {
        static void Postfix(ref RhythmCanvasCamFollower __instance)
        {
            if (UNANIMATED.effectsEnabled && UIController.forceLockedUI)
            {
                if (__instance.name != "DelayFollowCanvas(bg)")
                {
                    __instance.transform.localPosition = __instance.offset ? (__instance.goToTarget - __instance._firstCameraPos) : __instance.goToTarget;
                    if (__instance.name == "UiParentCanvas" && __instance.transform.childCount > 0)
                    {
                        __instance.transform.GetChild(0).localRotation = RhythmCamera.instance.gameObject.transform.localRotation;

                        // tan(theta) / tan(30)
                        float multiplier = Mathf.Tan(CameraController.cameraFOV / 2 * Mathf.Deg2Rad) / 0.57735f;
                        // __instance.transform.localScale = new Vector3(multiplier, multiplier, 1) * UIController.Defaults.canvasCamFollowerScale;
                        __instance.transform.GetChild(0).localScale = new Vector3(multiplier, multiplier, 1);
                    }
                }
            }
        }
    }



    [HarmonyPatch(typeof(RhythmCamera))]
    [HarmonyPatch("GetOriginalPosition")]
    internal class SetTargetPointPatch
    {
        static bool Prefix(ref RhythmCamera __instance, ref Vector3 __result)
        {
            if (UNANIMATED.effectsEnabled && UIController.forceLockedUI)
            {
                __result = __instance.transform.localPosition;
                return false;
            }
            else return true;
        }
    }



    [HarmonyPatch(typeof(RhythmScoreDisplay))]
    [HarmonyPatch("FixedUpdate")]
    internal class ScoreDisplayFixedUpdatePatch
    {
        static bool Prefix(ref RhythmScoreDisplay __instance)
        {
            if (UNANIMATED.effectsEnabled && __instance.controller.score != null)
            {
                float currentAlpha = __instance.display.alpha;
                __instance.displayScore = Mathf.SmoothDamp(__instance.displayScore, __instance.controller.score.totalScore, ref __instance.dampVel, __instance.lerpTime);
                __instance.display.text = string.Format("<mspace=3.3>{0:0000000}", __instance.displayScore);
                __instance.display.color = Color.Lerp(__instance.display.color, Color.black, 10f * Time.fixedDeltaTime);
                __instance.display.transform.localScale = Vector3.SmoothDamp(__instance.display.transform.localScale, __instance.initialLocalScale, ref __instance.scaleVel, 0.05f);
                if (__instance.controller.score.totalScore != __instance.prevTotalScore)
                {
                    __instance.prevTotalScore = __instance.controller.score.totalScore;
                    __instance.display.color = new Color32(254, 76, 113, byte.MaxValue);
                    __instance.display.transform.localScale = new Vector3(__instance.initialLocalScale.x, __instance.initialLocalScale.y * 1.5f, __instance.initialLocalScale.z);
                }
                __instance.display.alpha = currentAlpha;
            }
            return false;
            // else return true;
        }
    }
}