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
        public const string PLUGIN_VERSION = "0.1.16";
        internal static new ManualLogSource Logger;


        // Global queue
        public static Queue<CommandEventInfo> events = [];
        public static List<CommandEventInfo> beatmapEvents = [];


        // States
        public static bool effectsEnabled;
        public static bool videoEnabled;
        public static bool effectsWereEnabled;


        // Configs
        public static ConfigEntry<bool> enableUNANIMATED;
        public static ConfigEntry<bool> enableSceneSwitching;


        // Songs
        public static List<BeatmapIndex.Song> customUNANIMATEDSongs = [];


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

            enableUNANIMATED = Config.Bind(
                "General",
                "EnableUNANIMATED",
                true,
                "A global toggle to allow UNANIMATED to run on supported custom charts."
            );

            enableSceneSwitching = Config.Bind(
                "General",
                "EnableSceneSwitching",
                true,
                "Enables UNANIMATED's scene switching feature. If disabled, it will use the default or currently selected scene.\nScene switching has a memory and performance impact so this is included in case that is not desired."
            );
            // Reload the songs
            enableSceneSwitching.SettingChanged += (s, r) =>
            {
                customUNANIMATEDSongs.ForEach((song) => song.forceStageScene = enableSceneSwitching.Value);
            };
        }

        // public static bool IsEnabledOnChart()
        // {
        //     return RhythmController.Instance?.beatmap?.events != null
        //         && RhythmController.Instance.beatmap.events.Count() > 0
        //         && UNANIMATED.beatmapEvents.Any(e => e.Command == ControlCommand.UNANIMATED && e.GetEnumParam<GeneralOptions>(0) == GeneralOptions.LegacyCameraUnits)
        // }
    }



    // *Stuff to do when the rhythm scene is first loaded to make the mod work
    [HarmonyPatch(typeof(RhythmController))]
    [HarmonyPatch("Awake")]
    internal class ControllerAwakePatch
    {
        static void Postfix(ref RhythmController __instance)
        {
            // Don't do stuff while preloading
            if (SceneController.preloadingScenes) return;

            // Don't do stuff if the mod is not enabled
            if (!UNANIMATED.enableUNANIMATED.Value) return;


            try
            {
                // Parse events
                UNANIMATED.events = new Queue<CommandEventInfo>(
                    __instance.beatmap.events
                    .Where(e => Enum.TryParse<ControlCommand>(e.eventType, out _) && !int.TryParse(e.eventType, out _))
                    .Select(e => new CommandEventInfo(e)));

                UNANIMATED.beatmapEvents = UNANIMATED.events.ToList();


                // Enable effects if custom chart and enable command is present
                if (JeffBezosController.rhythmProgression is ArcadeProgression arcadeProgression && arcadeProgression.isCustomChart)
                {
                    if (UNANIMATED.beatmapEvents.Any(e => e.Command == ControlCommand.UNANIMATED && e.GetEnumParam<GeneralOptions>(0) == GeneralOptions.Enable))
                    {

                        // Set state
                        UNANIMATED.effectsEnabled = true;
                        UNANIMATED.effectsWereEnabled = true;
                        UNANIMATED.videoEnabled = false;


                        // Check video
                        foreach (GeneralOptions option in UNANIMATED.events.Where(e => e.Command == ControlCommand.UNANIMATED).Select(e => e.GetEnumParam<GeneralOptions>(0)))
                        {
                            // Enum.TryParse(str, out GeneralOptions options);
                            if (option == GeneralOptions.ShowBackgroundVideo) UNANIMATED.videoEnabled = true;
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


                        // Check legacy camera units
                        if (UNANIMATED.beatmapEvents.Any(e => e.Command == ControlCommand.UNANIMATED && e.GetEnumParam<GeneralOptions>(0) == GeneralOptions.LegacyCameraUnits))
                        {
                            UNANIMATED.Logger.LogInfo("Using legacy camera units.");
                            CameraController.legacyCameraUnit = true;
                        }


                        // Start up NOISZ controller
                        NOISZStageController.Init(__instance);
                    }
                }
            }
            catch (Exception ex)
            {
                UNANIMATED.Logger.LogError($"Failed to initialize UNANIMATED! {ex}");
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
            // UNANIMATED.defaultStageScene = null;
            UNANIMATED.events = [];
            UNANIMATED.beatmapEvents = [];
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
                    ControlCommand commandType = currentCommandEvent.Command;
                    if (commandType == ControlCommand.None || commandType == ControlCommand.Enable || commandType == ControlCommand.UNANIMATED) continue;


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
                    UNANIMATED.Logger.LogWarning($"Failed to parse command at {currentCommandEvent.startTime} ms! {ex}");
                    // UNANIMATED.Logger.LogWarning($"{ex.StackTrace}");
                }
            }

            if (UNANIMATED.effectsEnabled && UNANIMATED.enableSceneSwitching.Value && NOISZStageController.succeededPreloading) NOISZStageController.ManualUpdate(__instance);
        }
    }
}