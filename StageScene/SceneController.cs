using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using System;
using System.Collections;
using Rhythm;
using Arcade.Unlockables;
using Effects.RhythmGameSpecific;
using Cinemachine;
using UnityEngine.Video;

namespace UNANIMATED.StageScene
{
    public static class SceneController
    {
        // Constants
        public static readonly string rhythmGameContainerName = "Rhythm Game Container";
        public static readonly string noiszStageName = "NOISZRhythm";

        public static readonly string maskCameraName = "SpriteMaskCam";
        public static readonly string controllerPositionsObjectName = "UNANIMATED Controller Positions";
        public static readonly string characterSpawnerName = "Arcade Character Spawner";
        public static readonly string[] disallowedObjectNames = ["Rhythm Game Container", "Arcade Character Spawner", "_CameraOperatorOffsetLookatTarget", "_CameraOperatorOffsetFollowTarget", "PersistentStorage"];
        public static readonly string[] skippedObjectNames = ["UNANIMATED Controller Positions", "MVPlayer", "Timer Canvas Object"];
        public static readonly string[] scenesWithNoBackgroundFade = ["DreamReflection", "GreenscreenRhythm", "PlaybackStage"];


        // State variables
        public static bool preloadingScenes;
        public static List<CommandEventInfo> sceneEvents;


        // Scene swapping variables
        public static string activeSceneName;
        public static GameObject rhythmGameContainer;
        public static GameObject characterSpawner;
        public static GameObject backgroundFade;
        public static Dictionary<string, Transform> rhythmGameTransforms = [];
        public static Dictionary<string, GameObject> sceneRootObjects = [];
        public static Dictionary<string, GameObject> sceneMaskCameraObjects = [];
        public static Dictionary<string, GameObject> sceneMainCameraObjects = [];
        public static RenderTexture maskCamTexture;
        // public static Dictionary<string, GameObject[]> sceneObjectsToDisable = [];
        // public static Dictionary<string, GameObject[]> sceneObjectsToEnable = [];


        public static void RunPreloadScenes()
        {
            sceneEvents = UNANIMATED.beatmapEvents.Where((e) => e.Command == ControlCommand.StageScene).ToList();

            if (sceneEvents != null && sceneEvents.Count() > 0)
            {
                if (UNANIMATED.enableSceneSwitching.Value)
                {
                    UNANIMATED.Logger.LogInfo("Stage scene commands detected in events! Preloading relevant scenes.");

                    // Set time scale before preloading var is set
                    Time.timeScale = 0;

                    // Ensure no awake calls execute and override the code that we need
                    preloadingScenes = true;

                    // Add current scene
                    sceneEvents.Add(new CommandEventInfo(new EventInfo() { eventParams = [SceneManager.GetActiveScene().name] }));

                    // Start tasks
                    UNANIMATED.Instance.StartCoroutine(PreloadScenes());
                    UNANIMATED.Instance.StartCoroutine(BlockWhilePreloading());
                }
                else
                {
                    UNANIMATED.Logger.LogInfo("Stage scene commands detected in events but stage switching is not enabled. Not preloading scenes.");
                }
            }
        }

        private static IEnumerator PreloadScenes()
        {
            yield return new WaitForEndOfFrame();

            // try
            // {

            // Set current state
            activeSceneName = SceneManager.GetActiveScene().name;
            rhythmGameContainer = GameObject.Find(rhythmGameContainerName);
            characterSpawner = GameObject.Find(characterSpawnerName);
            backgroundFade = RhythmController.Instance?.transform.root.Find("RhythmUI/RhythmCanvas/BackgroundFade")?.gameObject;

            GameObject controllerPosParent = new GameObject(controllerPositionsObjectName);


            // Load relevant scenes
            foreach (CommandEventInfo commandEvent in sceneEvents)
            {
                string sceneName = commandEvent.GetStringParam(0);
                bool isStartingScene = sceneName == activeSceneName;

                // Skip if scene has previously been processed
                if (sceneRootObjects.ContainsKey(sceneName)) continue;

                // Check if scene name is valid
                if (SceneUtility.GetBuildIndexByScenePath(sceneName) == -1)
                {
                    UNANIMATED.Logger.LogWarning($"A scene with the name \"{sceneName}\" does not exist! Skipping scene load.");
                    continue;
                }

                // Load scene if not current scene
                if (!isStartingScene)
                {
                    AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

                    while (!asyncLoad.isDone)
                    {
                        // JeffBezosController.SetTimeScale(0, 0);
                        yield return null;
                    }
                }

                // Set parameters
                JeffBezosController.SetTimeScale(0, 0);

                // Get the newly loaded scene reference
                Scene loadedScene = SceneManager.GetSceneByName(sceneName);
                if (loadedScene.IsValid())
                {
                    SceneManager.SetActiveScene(loadedScene);

                    GameObject[] rootObjects = loadedScene.GetRootGameObjects();
                    GameObject newRootObject = new GameObject($"UNANIMATED {sceneName} Root");


                    for (int i = 0; i < rootObjects.Count(); i++)
                    {
                        GameObject root = rootObjects[i];

                        // Set controller transform
                        if (root.name == rhythmGameContainerName)
                        {
                            GameObject newGo = new GameObject($"UNANIMATED {sceneName} Controller Position");

                            Transform transform = newGo.transform;
                            transform.parent = controllerPosParent.transform;
                            transform.position = root.transform.position;
                            transform.rotation = root.transform.rotation;
                            transform.localScale = root.transform.localScale;

                            rhythmGameTransforms.TryAdd(sceneName, transform);

                            // NOISZ stage preload stuff
                            if (sceneName == noiszStageName) NOISZStageController.SpecialPreloadNOISZStage(root);
                        }

                        // Set camera references
                        if (root.tag == "MainCamera")
                        {
                            sceneMainCameraObjects.TryAdd(sceneName, root);

                            GameObject maskCamObject = root.transform.Find(maskCameraName).gameObject;
                            // maskCamTexture = maskCamObject?.GetComponentInChildren<Camera>()?.activeTexture;
                            sceneMaskCameraObjects.TryAdd(sceneName, maskCamObject);

                            // Fix camera mouse movement
                            // I have no idea why this fixes it but it does
                            root.GetComponent<CinemachineBrain>()?.ManualUpdate();
                        }

                        if (root.activeSelf && !skippedObjectNames.Contains(root.name))
                        {
                            if (IsRootBackgroundObject(root.name))
                            {
                                // true = keep position
                                root.transform.SetParent(newRootObject.transform, true);
                            }
                            else if (!isStartingScene)
                            {
                                root.SetActive(false);
                            }
                        }

                    }

                    // Set and store the root object
                    newRootObject.SetActive(isStartingScene);
                    sceneRootObjects.TryAdd(sceneName, newRootObject);


                    UNANIMATED.Logger.LogInfo($"Preloaded scene {sceneName}!");

                    yield return new WaitForEndOfFrame();
                }
                else
                {
                    UNANIMATED.Logger.LogWarning($"Scene {sceneName} was not valid!");
                }
            }

            // Set active scene
            Scene startScene = SceneManager.GetSceneByName(activeSceneName);
            if (startScene.IsValid()) SceneManager.SetActiveScene(startScene);


            // Disable scene preloading
            preloadingScenes = false;

            // // Fix current stencil mask instance
            // RhythmStencilMasks masks = RhythmController.Instance?.GetComponentInChildren<RhythmStencilMasks>();
            // if (masks) masks._hasRhythmController = true;

            UNANIMATED.Logger.LogInfo("Successfully preloaded all scenes!");
            // }
            // catch (Exception ex)
            // {
            //     UNANIMATED.Logger.LogError($"Failed to preload scenes! {ex.Message}\n{ex.StackTrace}");
            // }
        }



        private static IEnumerator BlockWhilePreloading()
        {
            yield return new WaitForEndOfFrame();

            // Main loop
            while (preloadingScenes)
            {
                // JeffBezosController.SetTimeScale(0, 0);
                yield return null;
            }

            // Reset gameplay when done
            yield return new WaitForEndOfFrame();
            JeffBezosController.SetTimeScale(Mathf.Abs(RhythmController.Instance.timeScale), 0f);
            LevelManager.sceneHasLoaded = true;
        }



        public static void SwitchSceneCommand(CommandEventInfo currentCommand)
        {
            if (UNANIMATED.enableSceneSwitching.Value)
            {
                UNANIMATED.Logger.LogInfo($"Parsed stage scene command at {currentCommand.Time} ms: stage {currentCommand.ParamString}");

                string sceneName = currentCommand.GetStringParam(0);
                Scene sceneToSwitch = SceneManager.GetSceneByName(sceneName);
                if (sceneToSwitch.IsValid() && activeSceneName != sceneName)
                {
                    UNANIMATED.Logger.LogInfo($"Switching stage scene to {sceneName}.");

                    // // Disable the camera for a few frames to let it catch up and not flash stuff on screen
                    // if (sceneMainCameraObjects.TryGetValue(sceneName, out GameObject mainCamObject))
                    // {
                    //     Camera mainCam = mainCamObject.GetComponent<Camera>();
                    //     if (mainCam != null)
                    //     {
                    //         mainCamObject.SetActive(false);
                    //         UNANIMATED.Instance.StartCoroutine(WaitEnableCamera(mainCam, mainCam.cullingMask, mainCam.clearFlags, mainCamObject));
                    //         // mainCam.clearFlags = CameraClearFlags.Nothing;
                    //         // mainCam.cullingMask = 0;
                    //     }
                    // }

                    // Fix mask camera
                    if (sceneMaskCameraObjects.TryGetValue(sceneName, out GameObject maskCamObject))
                    {
                        RenderTexture tex = maskCamObject.GetComponent<Camera>().activeTexture;
                        if (tex != null) Shader.SetGlobalTexture(RenderStencilGenerator.MaskTexture, tex);
                    }

                    // disable old objects
                    if (sceneRootObjects.TryGetValue(activeSceneName, out GameObject pastObject))
                    {
                        pastObject.SetActive(false);
                    }

                    // Move objects
                    if (rhythmGameTransforms.TryGetValue(sceneName, out Transform target))
                    {
                        // move rhythm game container
                        if (rhythmGameContainer != null)
                        {
                            rhythmGameContainer.transform.position = target.position;
                            rhythmGameContainer.transform.rotation = target.rotation;
                        }
                        // move character spawner
                        if (characterSpawner != null)
                        {
                            characterSpawner.transform.position = target.position;
                            characterSpawner.transform.rotation = target.rotation;
                        }
                    }

                    // Fix background on scenes with no background fade
                    backgroundFade?.SetActive(!scenesWithNoBackgroundFade.Contains(sceneName));

                    // enable new objects
                    if (sceneRootObjects.TryGetValue(sceneName, out GameObject currentObject))
                    {
                        currentObject.SetActive(true);
                    }


                    // Set active scene name
                    activeSceneName = sceneName;

                    // Set active scene
                    SceneManager.SetActiveScene(sceneToSwitch);
                    // UNANIMATED.Logger.LogInfo($"Switching stage scene to {sceneName}.");


                    // Fix NOISZ stage things
                    NOISZStageController.SwitchNOISZ(sceneName == noiszStageName);
                }
                else if (activeSceneName == sceneName)
                {
                    UNANIMATED.Logger.LogInfo($"Stage scene {sceneName} is the same as current scene, ignoring.");
                }
                else
                {
                    UNANIMATED.Logger.LogWarning($"Stage scene command was given with invalid scene {sceneName}, ignoring.");
                }
            }
        }

        // private static IEnumerator WaitEnableCamera(Camera mainCam, int cullMask, CameraClearFlags clearFlags, GameObject mainCamObject)
        // {
        //     yield return new WaitForEndOfFrame();
        //     yield return new WaitForEndOfFrame();
        //     yield return new WaitForEndOfFrame();
        //     yield return new WaitForEndOfFrame();
        //     yield return new WaitForEndOfFrame();
        //     yield return new WaitForEndOfFrame();
        //     yield return new WaitForEndOfFrame();
        //     yield return new WaitForEndOfFrame();
        //     yield return new WaitForEndOfFrame();
        //     yield return new WaitForEndOfFrame();
        //     yield return new WaitForEndOfFrame();
        //     yield return new WaitForEndOfFrame();
        //     yield return new WaitForEndOfFrame();
        //     yield return new WaitForEndOfFrame();
        //     yield return new WaitForEndOfFrame();
        //     yield return new WaitForEndOfFrame();
        //     yield return new WaitForEndOfFrame();
        //     yield return new WaitForEndOfFrame();
        //     yield return new WaitForEndOfFrame();
        //     yield return new WaitForEndOfFrame();
        //     yield return new WaitForEndOfFrame();
        //     yield return new WaitForEndOfFrame();
        //     yield return new WaitForEndOfFrame();
        //     yield return new WaitForEndOfFrame();
        //     yield return new WaitForEndOfFrame();
        //     yield return new WaitForEndOfFrame();
        //     yield return new WaitForFixedUpdate();
        //     // mainCam.clearFlags = clearFlags;
        //     // mainCam.cullingMask = cullMask;
        // }


        private static bool IsRootBackgroundObject(string name)
        {
            foreach (string str in disallowedObjectNames)
            {
                if (name.StartsWith(str, StringComparison.Ordinal)) return false;
            }
            return true;
        }



        public static void Reset()
        {
            rhythmGameTransforms = [];
            preloadingScenes = false;
            activeSceneName = string.Empty;
            rhythmGameContainer = null;
            characterSpawner = null;
            sceneEvents = [];
            sceneRootObjects = [];
            sceneMaskCameraObjects = [];
            sceneMainCameraObjects = [];
            maskCamTexture = null;
        }
    }
}