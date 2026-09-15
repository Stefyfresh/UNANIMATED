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
using UNANIMATED.VideoPlayback;
using UNANIMATED.Gameplay;
using UNANIMATED.UI;
using UNANIMATED.StageScene;
using UNANIMATED.Visuals;
using BepInEx.Configuration;


namespace UNANIMATED
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInProcess("UNBEATABLE.exe")]
    public class UNANIMATED : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.stefyfresh.UNANIMATED";
        public const string PLUGIN_NAME = "Stefyfresh's UNANIMATED";
        public const string PLUGIN_VERSION = "0.1.12";
        internal static new ManualLogSource Logger;

        // Global queue
        // public static Queue<HitObjectInfo> commands = new Queue<HitObjectInfo>();
        public static Queue<CommandEventInfo> events = new Queue<CommandEventInfo>();


        // States (should be moved to control classes)
        public static bool effectsEnabled;
        public static bool videoEnabled;
        public static bool isControllingCamera;
        public static string defaultStageScene;
        public static bool effectsWereEnabled;

        // Configs
        public static ConfigEntry<bool> modEnabled;


        // Instance for getting the GameObject
        public static UNANIMATED Instance
        {
            get; private set;
        }

        private void Awake()
        {
            Logger = base.Logger;
            Logger.LogInfo($"Plugin {PLUGIN_GUID} is loaded!");
            var harmony = new Harmony(PLUGIN_GUID);
            harmony.PatchAll();

            Instance = this;

            modEnabled = Config.Bind(
                "General",
                "EnableUNANIMATED",
                true,
                "A global toggle to allow UNANIMATED to run on supported custom charts."
            );
        }
    }



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



    // *Stuff to do when the rhythm scene is first loaded to make the mod work
    [HarmonyPatch(typeof(RhythmController))]
    [HarmonyPatch("Awake")]
    internal class ControllerAwakePatch
    {
        static void Postfix(ref RhythmController __instance)
        {
            // Don't do stuff 
            if (SceneController.preloadingScenes) return;


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


                        // Get GameObjects
                        ShaderMaskingController.GetCorrectShaders();
                        VisualController.Init();


                        // Log success
                        UNANIMATED.Logger.LogInfo($"Effects and animations enabled for chart {__instance.beatmap.metadata.title}!");


                        // Preload scenes if necessary
                        SceneController.RunPreloadScenes();
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
            UNANIMATED.defaultStageScene = null;
            UNANIMATED.events = [];
            // UNANIMATED.commands = new Queue<HitObjectInfo>();

            // Reset control classes
            CameraController.Reset();
            Character.CharacterController.Reset();
            GameplayController.Reset();
            SceneController.Reset();
            UIController.Reset();
            VisualController.Reset();
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

                    // Ignore invalid commands
                    if (!Enum.TryParse(currentCommandEvent.eventType, out ControlCommand commandType) || commandType == ControlCommand.None || commandType == ControlCommand.Enable) continue;


                    // Camera control command
                    if (commandType == ControlCommand.Camera)
                    {
                        CameraController.ParseCommand(currentCommandEvent);
                    }
                    else if (commandType == ControlCommand.Character)
                    {
                        Character.CharacterController.ParseCommand(currentCommandEvent);
                    }
                    else if (commandType == ControlCommand.Gameplay)
                    {
                        GameplayController.ParseCommand(currentCommandEvent);
                    }
                    else if (commandType == ControlCommand.UI)
                    {
                        UIController.ParseCommand(currentCommandEvent);
                    }
                    else if (commandType == ControlCommand.StageScene)
                    {
                        SceneController.SwitchSceneCommand(currentCommandEvent);
                    }
                    else
                    {
                        UNANIMATED.Logger.LogInfo($"Parsed unsupported command at {currentCommandEvent.startTime} ms: {commandType} | {currentCommandEvent.ParamString}");
                    }
                }
                catch (Exception ex)
                {
                    UNANIMATED.Logger.LogWarning($"Failed to parse command at {currentCommandEvent.startTime} ms! {ex.Message}\n{ex.StackTrace}");
                    UNANIMATED.Logger.LogWarning($"{ex.StackTrace}");
                }
            }
        }
    }
}