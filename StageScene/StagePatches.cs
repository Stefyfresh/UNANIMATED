using System.Collections.Generic;
using System.Linq;
using Arcade.Unlockables;
using HarmonyLib;
using Overworld;
using Rhythm;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UNANIMATED.StageScene
{
    // [HarmonyPatch(typeof(BeatmapParserEngine))]
    // [HarmonyPatch("ParseLineGeneral")]
    // internal class ParseLineGeneralPatch
    // {
    //     static void Postfix(string line)
    //     {
    //         string[] array = line.Split(':');

    //         if (array[0] == "DefaultStageScene")
    //         {
    //             UNANIMATED.defaultStageScene = array[1].Trim();
    //         }
    //     }
    // }



    [HarmonyPatch(typeof(BeatmapParserEngine))]
    [HarmonyPatch("ParseLine")]
    internal class ParseLinePatch
    {
        static void Postfix(ref BeatmapParserEngine __instance, string line, string sectionName, ref Beatmap beatmap)
        {
            if (sectionName == "Events")
            {
                __instance.ParseLineEvents(line, ref beatmap.events);
            }
        }
    }



    [HarmonyPatch(typeof(BeatmapIndex.Song))]
    [HarmonyPatch("AddBeatmapToCustomSong")]
    internal class AddBeatmapToCustomSongPatch
    {
        private static void Postfix(ref BeatmapIndex.Song __instance, Beatmap beatmap)
        {
            try
            {
                if (UNANIMATED.enableUNANIMATED.Value && beatmap.events != null && beatmap.events.Count > 0 && beatmap.events.Any(CommandEventInfo.IsEnableCommand))
                {
                    // Get the default stage event if it exists and get the stage name
                    EventInfo defaultStageEvent = beatmap.events.Find((e) => e.eventType == ControlCommand.UNANIMATED.ToString() && e.eventParams != null && e.eventParams.ElementAtOrDefault(0).StartsWith(GeneralOptions.DefaultStageScene.ToString()));

                    if (defaultStageEvent != default)
                    {
                        string defaultStageScene = new CommandEventInfo(defaultStageEvent).GetStringParam(1);

                        // Check if scene exists and apply if so, or set to train station
                        bool existsScene = RhythmSceneIndex.CachedDefaultIndex.GetAllRhythmScenes().Exists((rhythmScene) =>
                        {
                            if (rhythmScene.scene == defaultStageScene) return true;
                            string[] strings = rhythmScene.scene.Split("/");

                            if (strings[strings.Count() - 1] == defaultStageScene) return true;
                            return false;
                        });

                        if (existsScene)
                        {
                            UNANIMATED.Logger.LogInfo($"Setting stage to {defaultStageScene} on song {beatmap.metadata.title}");

                            __instance.stageScene = defaultStageScene;
                            __instance.forceStageScene = UNANIMATED.enableSceneSwitching.Value;
                        }
                        else
                        {
                            UNANIMATED.Logger.LogWarning($"Invalid rhythm scene \"{defaultStageScene}\" parsed for song {beatmap.metadata.title}!");

                            // __instance.stageScene = "TrainStationRhythm";
                            // __instance.forceStageScene = UNANIMATED.enableSceneSwitching.Value;
                        }
                        UNANIMATED.customUNANIMATEDSongs.Remove(__instance);
                        UNANIMATED.customUNANIMATEDSongs.Add(__instance);
                    }
                }

            }
            catch (System.Exception ex)
            {
                UNANIMATED.Logger.LogWarning($"Error setting stage on song {beatmap.metadata.title}! {ex.Message}");
            }
        }
    }



    // Block level from finishing loading if scenes are being loaded
    [HarmonyPatch(typeof(LevelManager))]
    [HarmonyPatch("OnSceneLoaded")]
    internal class OnSceneLoadedPatch
    {
        static void Postfix()
        {
            // if (song.stageScene != "TrainStationRhythm" && song.)
            if (SceneController.preloadingScenes && LevelManager.sceneHasLoaded == true) LevelManager.sceneHasLoaded = false;
        }
    }



    // Fix playback stage crashing
    [HarmonyPatch(typeof(RhythmMVPlayer))]
    [HarmonyPriority(Priority.First)]
    [HarmonyPatch("SetVideo")]
    internal class SetVideoPatch
    {
        static bool Prefix(ref RhythmMVPlayer __instance)
        {
            if (UNANIMATED.effectsEnabled && SceneController.preloadingScenes) __instance.controller = RhythmController.Instance;
            return true;
        }
    }



    // Fix playback stage not working
    [HarmonyPatch(typeof(RhythmMVPlayer))]
    [HarmonyPatch("OnDisable")]
    internal class VideoOnDisablePatch
    {
        static bool Prefix()
        {
            // Prevent the delegates from unsubscribing and not starting playback
            if (UNANIMATED.effectsEnabled) return false;
            else return true;
        }
    }



    // Fix playback stage bugs
    [HarmonyPatch(typeof(RhythmMVPlayer))]
    [HarmonyPatch("Update")]
    internal class RhythmMVPlayerUpdatePatch
    {
        static bool Prefix(ref RhythmMVPlayer __instance)
        {
            if (UNANIMATED.effectsEnabled && __instance.controller && __instance.controller.songTracker.Position > __instance._song.VideoStartTime && __instance.controller.songTracker.Position >= 0f && !__instance._isPlaying)
            {
                __instance.player.Play();
                // if (__instance.controller.songTracker.Position > __instance.player.time) __instance.player.time = __instance.controller.songTracker.Position;
                __instance._isPlaying = true;
            }
            return true;
        }
    }



    // Fix crashes in the other instances of RhythmStencilMasks
    [HarmonyPatch(typeof(RhythmStencilMasks))]
    [HarmonyPriority(Priority.First)]
    [HarmonyPatch("Start")]
    internal class RhythmStencilMasksCrashFix
    {
        static bool Prefix(ref RhythmStencilMasks __instance)
        {
            if (UNANIMATED.effectsEnabled && SceneController.preloadingScenes) __instance._hasRhythmController = false;
            return true;
        }
    }



    // Stop timeScale from accepting input when scene is preloading 
    [HarmonyPatch(typeof(Time))]
    [HarmonyPriority(Priority.First)]
    [HarmonyPatch("timeScale", MethodType.Setter)]
    internal class TimeScalePatch
    {
        static bool Prefix()
        {
            return !(UNANIMATED.effectsEnabled && SceneController.preloadingScenes);
        }
    }



    // Stop the DayNightSystem from incorrectly setting the render settings for the wrong scene
    [HarmonyPatch(typeof(DayNightSystem))]
    [HarmonyPatch("UpdateDayTimeSettings")]
    internal class DayNightSystemPatch
    {
        static bool Prefix(ref DayNightSystem __instance)
        {
            return __instance.gameObject.scene == SceneManager.GetActiveScene();
        }
    }
}