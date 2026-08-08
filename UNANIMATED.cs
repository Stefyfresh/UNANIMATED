using BepInEx;
using BepInEx.Logging;
using System.Collections.Generic;
using HarmonyLib;
using Rhythm;
using DG.Tweening;
using UnityEngine;
using Arcade.UI.SongSelect;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using UNANIMATED.CameraControl;
using UNANIMATED.Character;


namespace UNANIMATED
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInProcess("UNBEATABLE.exe")]
    public class UNANIMATED : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "net.stefyfresh.UNANIMATED";
        public const string PLUGIN_NAME = "Stefyfresh's UNANIMATED";
        public const string PLUGIN_VERSION = "0.1.6";
        internal static new ManualLogSource Logger;
        public static Queue<HitObjectInfo> commands = new Queue<HitObjectInfo>();
        public static Queue<CommandEventInfo> events = new Queue<CommandEventInfo>();

        public static bool effectsEnabled;
        public static bool videoEnabled;
        public static bool isControllingCamera;
        public static string defaultStageScene;
        public static bool effectsWereEnabled;

        private void Awake()
        {
            Logger = base.Logger;
            Logger.LogInfo($"Plugin {PLUGIN_GUID} is loaded!");
            var harmony = new Harmony(PLUGIN_GUID);
            harmony.PatchAll();
        }
    }



    // *Stuff to do when the rhythm scene is first loaded to make the mod work
    [HarmonyPatch(typeof(RhythmController))]
    [HarmonyPatch("Awake")]
    internal class ControllerAwakePatch
    {
        static void Postfix(ref RhythmController __instance)
        {
            // Parse events
            UNANIMATED.events = new Queue<CommandEventInfo>(
                __instance.beatmap.events
                .Where(e => Enum.TryParse<ControlCommand>(e.eventType, out _) && !int.TryParse(e.eventType, out _))
                .Select(e => new CommandEventInfo(e)));


            // Enable effects if custom chart and enable command is present
            if (JeffBezosController.rhythmProgression is ArcadeProgression arcadeProgression && arcadeProgression.isCustomChart)
            {
                bool enableHitObject = __instance.beatmap.commands.Count > 0 && __instance.beatmap.commands.First().lane == 2 && __instance.beatmap.commands.First().whistle && __instance.beatmap.commands.First().clap && __instance.beatmap.commands.First().finish;
                bool enableEvent = UNANIMATED.events.Count > 0 && UNANIMATED.events.First().eventType == "Enable";
                if (enableHitObject || enableEvent)
                {
                    try
                    {



                        // Dequeue start command
                        if (enableHitObject) UNANIMATED.commands.Dequeue();
                        // if (enableEvent) UNANIMATED.events.Dequeue();


                        // Set state
                        UNANIMATED.effectsEnabled = true;
                        UNANIMATED.effectsWereEnabled = true;
                        UNANIMATED.videoEnabled = false;


                        // Check video
                        if (enableEvent)
                        {
                            foreach (string str in UNANIMATED.events.First().eventParams)
                            {
                                Enum.TryParse(str, out GeneralOptions options);
                                if (options == GeneralOptions.BackgroundVideo) UNANIMATED.videoEnabled = true;
                            }
                        }

                        // set up characters
                        GameObject charSpawnerGO = GameObject.Find("Arcade Character Spawner");
                        Character.CharacterController.Reset();
                        Character.CharacterController.spawner = charSpawnerGO.GetComponent<RhythmCharacterSelector>();





                        // Log success
                        UNANIMATED.Logger.LogInfo($"Effects and animations enabled for chart {__instance.beatmap.metadata.title}!");
                    }
                    catch (Exception ex)
                    {
                        UNANIMATED.Logger.LogError($"Failed to initialize UNANIMATED! {ex.Message}");
                        UNANIMATED.Logger.LogError($"{ex.StackTrace}");
                    }
                }
            }
        }
    }


    // *Stuff to do the rhythm scene is unloaded to reset the mod
    [HarmonyPatch(typeof(RhythmController))]
    [HarmonyPatch("OnDestroy")]
    internal class ControllerOnDestroyPatch
    {
        static void Postfix()
        {
            // Reset variables
            UNANIMATED.videoEnabled = false;
            UNANIMATED.effectsEnabled = false;
            CameraController.Reset();
            Character.CharacterController.Reset();
            UNANIMATED.defaultStageScene = null;
            UNANIMATED.commands = new Queue<HitObjectInfo>();
        }
    }



    // *Global command parsing code loop
    [HarmonyPatch(typeof(RhythmController))]
    [HarmonyPatch("UpdateCommands")]
    internal class UpdateCommandsPatch
    {
        static void Postfix(ref RhythmController __instance)
        {
            // Return if not enabled
            if (!UNANIMATED.effectsEnabled) return;


            // Process latest command
            HitObjectInfo currentCommand;
            while (UNANIMATED.commands.Count > 0 && (currentCommand = UNANIMATED.commands.Peek()) != null && __instance.songTracker.Position >= currentCommand.time)
            {
                try
                {
                    // dequeue command and perform logic
                    UNANIMATED.commands.Dequeue();

                    // Ignore regular notes
                    if (currentCommand.hitSound == (int)ControlCommand.None) continue;

                    // Camera control command
                    if ((int)currentCommand.hitSound == (int)ControlCommand.Camera)
                    {
                        CameraController.ParseCameraCommand(currentCommand);
                    }
                    else
                    {
                        UNANIMATED.Logger.LogInfo($"Parsed unsupported command at {currentCommand.time} ms: {(ControlCommand)currentCommand.hitSound} | {string.Join(", ", currentCommand.hitSample)}");
                    }
                }
                catch (Exception ex)
                {
                    UNANIMATED.Logger.LogError($"Failed to parse command at {currentCommand.time} ms! {ex.Message}");
                    // UNANIMATED.Logger.LogError($"{ex.Message}");

                }
            }


            CommandEventInfo currentCommandEvent;
            CommandEventInfo pastCommandEvent = null;
            while (UNANIMATED.events.Count > 0 && (currentCommandEvent = UNANIMATED.events.Peek()) != null && __instance.songTracker.Position >= currentCommandEvent.startTime)
            {
                try
                {
                    // check for duplicate events
                    if (currentCommandEvent.CommandsEqual(pastCommandEvent))
                    {
                        UNANIMATED.events.Dequeue();
                        continue;
                    }

                    // dequeue command and perform logic
                    pastCommandEvent = UNANIMATED.events.Dequeue();

                    // Ignore invalid
                    if (!Enum.TryParse(currentCommandEvent.eventType, out ControlCommand commandType) || commandType == ControlCommand.None || commandType == ControlCommand.Enable) continue;

                    // Camera control command
                    if (commandType == ControlCommand.Camera)
                    {
                        // TODO: fix yucky code that I put in temporarily so I wouldn't have to rewrite the parsing function
                        // if (float.TryParse(currentCommandEvent.eventParams[0], out _))
                        // {
                        HitObjectInfo hitObjectEvent = new()
                        {
                            time = currentCommandEvent.startTime,
                            type = currentCommandEvent.HasEndTime ? 128 : 1,
                            hitSample = currentCommandEvent.Parameters,
                            objectParams = [$"{currentCommandEvent.EndTime}"]
                        };
                        if (Enum.TryParse(hitObjectEvent.hitSample[0], out CameraOverride parsed))
                        {
                            hitObjectEvent.hitSample[0] = $"{(int)parsed}";
                        }
                        try
                        {
                            if (Enum.TryParse(hitObjectEvent.hitSample[1], out CameraPoint parsed2))
                            {
                                hitObjectEvent.hitSample[1] = $"{(int)parsed2}";
                            }
                            if (Enum.TryParse(hitObjectEvent.hitSample[1], out Ease parsed3))
                            {
                                hitObjectEvent.hitSample[1] = $"{(int)parsed3}";
                            }
                        }
                        catch (Exception)
                        {
                            hitObjectEvent.hitSample = [hitObjectEvent.hitSample[0], "0"];
                        }
                        CameraController.ParseCameraCommand(hitObjectEvent);
                        // }
                    }
                    else if (commandType == ControlCommand.Character)
                    {
                        Character.CharacterController.ParseCharacterCommand(currentCommandEvent);
                    }
                    else
                    {
                        UNANIMATED.Logger.LogInfo($"Parsed unsupported command at {currentCommandEvent.startTime} ms: {commandType} | {string.Join(", ", currentCommandEvent.eventParams)}");
                    }
                }
                catch (Exception ex)
                {
                    UNANIMATED.Logger.LogError($"Failed to parse command at {currentCommandEvent.startTime} ms! {ex.Message}");
                    // UNANIMATED.Logger.LogError($"{ex.StackTrace}");
                }
            }
        }
    }
}