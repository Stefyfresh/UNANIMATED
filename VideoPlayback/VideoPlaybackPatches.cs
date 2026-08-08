using System.IO;
using HarmonyLib;
using Rhythm;
using UnityEngine.Video;

namespace UNANIMATED.VideoPlayback
{

    [HarmonyPatch(typeof(RhythmStencilMasks))]
    [HarmonyPatch("Init")]
    internal class RhythmStencilMasksInitPatch
    {
        static void Postfix(ref RhythmStencilMasks __instance)
        {
            if (UNANIMATED.effectsEnabled && UNANIMATED.videoEnabled && JeffBezosController.rhythmProgression is ArcadeProgression progression)
            {
                // Play video if it exists
                if (progression.customVideoPath != null && File.Exists(progression.customVideoPath) && UNANIMATED.effectsEnabled)
                {
                    UNANIMATED.Logger.LogInfo("Custom video found and video playback is enabled! Playing video.");
                    __instance.ResetMaskUpdate();

                    __instance.video.source = VideoSource.Url;
                    __instance.video.url = "file:///" + progression.customVideoPath.Replace("\\", "/");
                    __instance.video.Play();
                    __instance.playOnSongStart = true;
                    __instance.videoRenderTexture.color = __instance.videoColor;
                }
            }
        }
    }



    // [HarmonyPatch(typeof(RhythmStencilMasks))]
    // [HarmonyPatch("Update")]
    // internal class StencilUpdatePatch
    // {
    //     static void Postfix(ref RhythmStencilMasks __instance)
    //     {
    //         if (UNANIMATED.effectsEnabled && UNANIMATED.videoEnabled)
    //         {
    //             // UNANIMATED.Logger.LogInfo($"{__instance.videoRenderTexture.color} | {__instance.video.isPlaying} | {__instance.controller.songTracker.TimelinePosition:0}");
    //         }
    //     }
    // }

}