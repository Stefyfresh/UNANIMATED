using System.IO;
using HarmonyLib;
using Rhythm;
using UNANIMATED.StageScene;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace UNANIMATED.VideoPlayback
{
    [HarmonyPatch(typeof(RhythmStencilMasks))]
    [HarmonyPatch(nameof(RhythmStencilMasks.Init))]
    internal class RhythmStencilMasksInitPatch
    {
        static void Postfix(ref RhythmStencilMasks __instance)
        {
            if (UNANIMATED.effectsEnabled && UNANIMATED.videoEnabled && JeffBezosController.rhythmProgression is ArcadeProgression progression && __instance.gameObject.scene == SceneController.primaryScene)
            {
                // Play video if it exists
                if (progression.customVideoPath != null && File.Exists(progression.customVideoPath) && UNANIMATED.effectsEnabled)
                {
                    UNANIMATED.Logger.LogInfo("Custom video found and video playback is enabled! Playing video.");

                    // Set shader
                    VideoPlaybackController.FixVideoPlayer(__instance.transform.Find("Video Player").gameObject);

                    VideoPlaybackController.showVideo = true;

                    __instance.ResetMaskUpdate();
                    __instance.video.source = VideoSource.Url;
                    __instance.video.url = "file:///" + progression.customVideoPath.Replace("\\", "/");
                    __instance.video.Play();
                    __instance.video.timeReference = VideoTimeReference.ExternalTime;
                    __instance.video.audioOutputMode = VideoAudioOutputMode.None;
                    __instance.video.aspectRatio = VideoAspectRatio.FitInside;
                    __instance.videoRenderTexture.texture = __instance.video.targetTexture = VideoPlaybackController.CreateRenderTexture();
                    __instance.playOnSongStart = true;
                    __instance.videoRenderTexture.color = __instance.videoColor;
                    VideoPlaybackController.player = __instance.video;
                }
            }
        }
    }



    [HarmonyPatch(typeof(RhythmStencilMasks))]
    [HarmonyPatch(nameof(RhythmStencilMasks.Update))]
    internal class StencilUpdatePatch
    {
        static void Postfix(ref RhythmStencilMasks __instance)
        {
            if (UNANIMATED.effectsEnabled && UNANIMATED.videoEnabled && __instance.gameObject.scene == SceneController.primaryScene)
            {
                __instance.video.externalReferenceTime = __instance.controller.songTracker.Position / 1000f;

                if (VideoPlaybackController.showVideo != __instance.videoRenderTexture.enabled) __instance.videoRenderTexture.enabled = VideoPlaybackController.showVideo;
            }
        }
    }

}