using HarmonyLib;
using Rhythm;

namespace UNANIMATED.CameraControl
{

    [HarmonyPatch(typeof(RhythmCamera))]
    [HarmonyPatch("FixedUpdate")]
    internal class CameraFixedUpdatePatch
    {
        static bool Prefix(ref RhythmCamera __instance)
        {
            if (!UNANIMATED.effectsEnabled) return true;

            CameraController.DoFixedUpdate(__instance);
            return false;
        }
    }



    [HarmonyPatch(typeof(RhythmCamera))]
    [HarmonyPatch("SetTargetPoint")]
    internal class SetTargetPointPatch
    {
        static bool Prefix()
        {
            CameraController.requestingCameraPosChange = true;

            // Disable camera control from RhythmController when UNANIMATED is in charge
            if (UNANIMATED.isControllingCamera) return false;
            else return true;
        }
    }



    // !CAMERA SHAKE
    // Format: duration, amount
    // Default hit: 0.01, 0.01
    // dodge miss: 0.15, 0.03
    // Freestyle hit: 0.15, 0.01
    // Controller hit: 0.02, 0.05
    // Constant camera shake: 0.01, 0.02
}