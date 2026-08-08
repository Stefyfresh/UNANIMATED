using System.Collections.Generic;
using System.Linq;
using Arcade.Unlockables;
using HarmonyLib;
using Rhythm;

namespace UNANIMATED.StageScene
{
    [HarmonyPatch(typeof(BeatmapParserEngine))]
    [HarmonyPatch("ParseLineGeneral")]
    internal class ParseLineGeneralPatch
    {
        static void Postfix(string line)
        {
            string[] array = line.Split(':');

            if (array[0] == "DefaultStageScene")
            {
                UNANIMATED.defaultStageScene = array[1].Trim();
            }
        }
    }



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
}