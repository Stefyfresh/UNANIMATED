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

namespace UNANIMATED.StageScene
{
    public static class SceneController
    {
        // Constants
        public static readonly string rhythmGameContainerName = "Rhythm Game Container";
        public static readonly string maskCameraName = "SpriteMaskCam";
        public static readonly string controllerPositionsObjectName = "UNANIMATED Controller Positions";
        public static readonly string defaultRhythmScene = "TrainStationRhythm";
        public static readonly string characterSpawnerName = "Arcade Character Spawner";
        public static readonly string[] disallowedObjectNames = ["Rhythm Game Container", "Arcade Character Spawner", "_CameraOperatorOffsetLookatTarget", "_CameraOperatorOffsetFollowTarget", "PersistentStorage"];
        public static readonly string[] skippedObjectNames = ["UNANIMATED Controller Positions", "MVPlayer", "Timer Canvas Object"];


        // State variables
        public static bool preloadingScenes;
        public static List<CommandEventInfo> sceneEvents;


        // Scene swapping variables
        public static string activeSceneName;
        public static GameObject rhythmGameContainer;
        public static GameObject characterSpawner;
        public static Dictionary<string, Transform> rhythmGameTransforms = [];
        public static Dictionary<string, GameObject> sceneRootObjects = [];
        public static Dictionary<string, GameObject> sceneMaskCameraObjects = [];
        public static Dictionary<string, GameObject> sceneMainCameraObjects = [];
        public static RenderTexture maskCamTexture;
        // public static Dictionary<string, GameObject[]> sceneObjectsToDisable = [];
        // public static Dictionary<string, GameObject[]> sceneObjectsToEnable = [];


        public static void RunPreloadScenes()
        {
            sceneEvents = UNANIMATED.events.Where((e) => e.eventType == "StageScene").ToList();

            if (sceneEvents != null && sceneEvents.Count() > 0)
            {
                if (FileStorage.beatmapOptions.CurrentRhythmScene == string.Empty)
                {
                    UNANIMATED.Logger.LogInfo("Stage scene commands detected in events! Preloading relevant scenes.");

                    // Ensure no awake calls execute and override the code that we need
                    preloadingScenes = true;

                    // Add current scene
                    sceneEvents.Add(new CommandEventInfo(new EventInfo() { eventParams = [defaultRhythmScene] }));

                    // Start tasks
                    UNANIMATED.Instance.StartCoroutine(PreloadScenes());
                    UNANIMATED.Instance.StartCoroutine(BlockWhilePreloading());
                }
                else
                {
                    UNANIMATED.Logger.LogInfo("Stage scene commands detected in events but player's stage is not default. Not preloading scenes.");
                }
            }
        }

        private static IEnumerator PreloadScenes()
        {
            yield return new WaitForEndOfFrame();

            // try
            // {

            // Set current state
            activeSceneName = defaultRhythmScene;
            rhythmGameContainer = GameObject.Find(rhythmGameContainerName);
            characterSpawner = GameObject.Find(characterSpawnerName);

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
                        JeffBezosController.SetTimeScale(0, 0);
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

                    // TODO: FIX NOISZ STAGE

                    // Set and store the root object
                    newRootObject.SetActive(isStartingScene);
                    sceneRootObjects.TryAdd(sceneName, newRootObject);


                    UNANIMATED.Logger.LogInfo($"Preloaded scene {sceneName}!");
                }
                else
                {
                    UNANIMATED.Logger.LogWarning($"Scene {sceneName} was not valid!");
                }
            }

            // Set active scene
            Scene startScene = SceneManager.GetSceneByName(activeSceneName);
            if (startScene.IsValid()) SceneManager.SetActiveScene(startScene);

            // // Set the correct texture for the cameras
            // for (int i = 0; i < sceneMaskCameraObjects.Values.Count(); i++)
            // {
            //     Camera maskCamera = sceneMaskCameraObjects.Values.ElementAt(i)?.GetComponent<Camera>();
            //     if (maskCamera != null) maskCamera.targetTexture = maskCamTexture;
            // }

            // Disable scene preloading
            preloadingScenes = false;

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
                JeffBezosController.SetTimeScale(0, 0);
                yield return null;
            }

            // Reset gameplay when done
            yield return new WaitForEndOfFrame();
            JeffBezosController.SetTimeScale(Mathf.Abs(RhythmController.Instance.timeScale), 0f);
            LevelManager.sceneHasLoaded = true;
        }



        public static void SwitchSceneCommand(CommandEventInfo currentCommand)
        {
            UNANIMATED.Logger.LogInfo($"Parsed stage command at {currentCommand.Time} ms: stage {currentCommand.ParamString} | length {currentCommand.Duration:0} ms");

            if (FileStorage.beatmapOptions.CurrentRhythmScene == string.Empty)
            {
                string sceneName = currentCommand.GetStringParam(0);
                Scene sceneToSwitch = SceneManager.GetSceneByName(sceneName);
                if (sceneToSwitch.IsValid())
                {
                    UNANIMATED.Logger.LogInfo($"Switching stage scene to {sceneName}.");

                    // // disable new camera
                    // if (sceneMainCameraObjects.TryGetValue(sceneName, out GameObject mainCamObject))
                    // {
                    //     mainCamObject.SetActive(false);
                    //     UNANIMATED.Instance.StartCoroutine(WaitEnableCamera(mainCamObject));
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

                    // Fix background on dream stage
                    RhythmController.Instance?.transform.root.Find("RhythmUI/RhythmCanvas/BackgroundFade")?.gameObject.SetActive(sceneName != "DreamReflection");

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
                }
                else
                {
                    UNANIMATED.Logger.LogWarning($"Stage scene command was given with invalid scene {sceneName}, ignoring.");
                }
            }
        }

        // private static IEnumerator WaitEnableCamera(GameObject mainCamObject)
        // {
        //     yield return new WaitForEndOfFrame();
        //     yield return new WaitForEndOfFrame();
        //     yield return new WaitForEndOfFrame();
        //     yield return new WaitForEndOfFrame();
        //     yield return new WaitForEndOfFrame();
        //     yield return new WaitForEndOfFrame();
        //     yield return new WaitForFixedUpdate();
        //     mainCamObject.SetActive(true);
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