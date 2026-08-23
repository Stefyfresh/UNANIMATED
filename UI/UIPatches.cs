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
                    __instance.transform.localRotation = RhythmCamera.instance.gameObject.transform.localRotation;

                    // tan(theta) / tan(30)
                    float multiplier = Mathf.Tan(CameraController.cameraFOV / 2 * Mathf.Deg2Rad) / 0.57735f;
                    __instance.transform.localScale = new Vector3(multiplier, multiplier, 1) * UIController.Defaults.canvasCamFollowerScale;
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
}