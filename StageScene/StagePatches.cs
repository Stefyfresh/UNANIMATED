using System.Collections.Generic;
using System.Linq;
using Arcade.Unlockables;
using HarmonyLib;
using Rhythm;

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
            if (UNANIMATED.defaultStageScene != null)
            {
                bool existsScene = RhythmSceneIndex.CachedDefaultIndex.GetAllRhythmScenes().Exists((rhythmScene) =>
                {
                    if (rhythmScene.scene == UNANIMATED.defaultStageScene) return true;
                    string[] strings = rhythmScene.scene.Split("/");

                    if (strings[strings.Count() - 1] == UNANIMATED.defaultStageScene) return true;
                    return false;
                });

                if (existsScene)
                {
                    UNANIMATED.Logger.LogInfo($"Setting stage to \"{UNANIMATED.defaultStageScene}\" on song \"{beatmap.metadata.titleUnicode}\"");

                    __instance.stageScene = UNANIMATED.defaultStageScene;
                    // __instance.forceStageScene = true; //TODO: MAKE THIS A SETTING
                }
                else
                {
                    UNANIMATED.Logger.LogWarning($"Invalid rhythm scene \"{UNANIMATED.defaultStageScene}\" parsed for song \"{beatmap.metadata.titleUnicode}!\"");
                }
                UNANIMATED.defaultStageScene = null;


                // string.Join(",\n", Arcade.Unlockables.RhythmSceneIndex.CachedDefaultIndex.GetAllRhythmScenes().Select(s => s.scene).ToArray())
            }
        }
    }



    // [HarmonyPatch(typeof(LevelManager))]
    // [HarmonyPatch("LoadCustomArcadeLevel")]
    // internal class LoadCustomArcadeLevelPatch
    // {
    //     static void Postfix(BeatmapIndex.Song song, string customScene = "")
    //     {
    //         if (song.stageScene != "TrainStationRhythm" && song.)
    //     }
    // }


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
}