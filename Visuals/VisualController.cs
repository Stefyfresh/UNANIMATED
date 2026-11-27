using System.Collections.Generic;
using System.Linq;
using Overworld;
using Rhythm;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UNANIMATED.Visuals
{
    public static class VisualController
    {
        // ---- Materials ---- 
        public static Material invertMaterial;
        // Above scene elements, guides, speed lines, measure bars, player, below everything else

        public static Material backgroundMaterial;
        // Above scene elements, speed lines, measure bars, below everything else

        public static Material scorecardMaterial;
        // Below vignette and score, above everything else

        public static Material twinBarMaterial;
        // Below vignette and score, above everything else 

        // Day Night Weather stuff
        public static List<DayNightSystem> dayNightSystems = [];
        public static Dictionary<string, Weather> sceneDefaultWeather = [];
        public static Dictionary<string, Light> sceneSuns = [];
        public static DayNightSystem masterDayNightSystem;
        public static Light masterSun;

        // States
        public static bool isInverted;
        public static TimeAndWeatherOption currentTimeAndWeather;
        public static float currentCloudRotation;

        public static void Init(RhythmController controller)
        {
            Reset();
            SetSceneDefaultWeather(SceneManager.GetActiveScene().name);
        }


        public static void GetDayNightSystems()
        {
            dayNightSystems = Object.FindObjectsByType<DayNightSystem>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID).Cast<DayNightSystem>().ToList();

            foreach (DayNightSystem dayNight in dayNightSystems)
            {
                dayNight.RefreshOnEnable = true;
                dayNight.enabled = true;
                dayNight.UpdateDayTimeSettings(true);
            }

            if (dayNightSystems.Count > 0)
            {
                // The DreamReflection DayNightSystem is best, use that if possible
                DayNightSystem dreamReflectionDayNight = dayNightSystems.FirstOrDefault(dayNight => dayNight.gameObject.scene.name == "DreamReflection");

                if (dreamReflectionDayNight) masterDayNightSystem = Object.Instantiate(dreamReflectionDayNight);
                else masterDayNightSystem = Object.Instantiate(dayNightSystems.First());

                masterSun = masterDayNightSystem.GetComponent<Light>();
                // if (sun) sun.enabled = false;

                masterDayNightSystem.gameObject.name = "UNANIMATED Master DayNightSystem";
                SetTimeAndWeather(TimeAndWeatherOption.Reset, 0);
            }
        }


        public static void Reset()
        {
            isInverted = false;
            dayNightSystems = [];
            sceneDefaultWeather = [];
            masterDayNightSystem = null;
            currentTimeAndWeather = TimeAndWeatherOption.Reset;
            currentCloudRotation = 0;
        }

        public static void ParseCommand(CommandEventInfo currentCommand)
        {
            // Get relevant variables
            VisualOption type = currentCommand.GetEnumParamForced<VisualOption>(0);
            // bool enabled = currentCommand.GetBoolParam(1);
            float time = currentCommand.Duration / 1000f;

            // Logging
            UNANIMATED.Logger.LogInfo($"Parsed visuals command at {currentCommand.Time} ms: {type} | params {currentCommand.ParamString} | length {currentCommand.Duration:0} ms");

            // Command logic
            switch (type)
            {
                case VisualOption.Reset:
                    Reset();
                    break;
                case VisualOption.TimeAndWeather:
                    SetTimeAndWeather(currentCommand.GetEnumParamForced<TimeAndWeatherOption>(1), time);
                    break;
                case VisualOption.CloudRotation:
                    SetCloudRotation(currentCommand.GetFloatParam(1), time);
                    break;


                    // case VisualOption.Invert:

                    //     break;
            }
        }


        public static void SetSceneDefaultWeather(string sceneName)
        {
            if (!sceneDefaultWeather.ContainsKey(sceneName))
            {
                Weather weather = new();

                if (RenderSettings.sun)
                {
                    weather.color = RenderSettings.sun.color;
                    weather.intensity = RenderSettings.sun.intensity;
                    weather.orientation = RenderSettings.sun.transform.eulerAngles;
                    sceneSuns.TryAdd(sceneName, RenderSettings.sun);
                }

                weather.skybox = RenderSettings.skybox;
                weather.bounceLight = RenderSettings.ambientLight;
                weather.fogColor = RenderSettings.fogColor;
                weather.fogDensity = RenderSettings.fogDensity;

                sceneDefaultWeather.TryAdd(sceneName, weather);
            }
        }


        public static void FixTimeAndWeatherForSceneSwitch()
        {
            SetTimeAndWeather(currentTimeAndWeather, 0);
        }


        private static void SetTimeAndWeather(TimeAndWeatherOption mode, float duration)
        {
            Scene activeScene = SceneManager.GetActiveScene();
            string sceneName = activeScene.name;

            // If there is a DayNightSystem in the current scene, only use that
            DayNightSystem dayNightSystemInCurrentScene = dayNightSystems.FirstOrDefault(dayNight => dayNight.gameObject.scene == activeScene);

            if (dayNightSystemInCurrentScene)
            {
                masterSun.enabled = false;
                SetTimeAndWeatherInternal(dayNightSystemInCurrentScene, mode, duration, sceneName);
            }
            else if (masterDayNightSystem)
            {
                if (mode == TimeAndWeatherOption.Reset) masterSun.enabled = false;
                else masterSun.enabled = true;
                SetTimeAndWeatherInternal(masterDayNightSystem, mode, duration, sceneName, masterSun);
            }

            currentTimeAndWeather = mode;
        }


        private static void SetTimeAndWeatherInternal(DayNightSystem dayNight, TimeAndWeatherOption mode, float duration, string sceneName, Light sun = null)
        {
            if (DayNightSystemExtension.weatherCoroutine != null) dayNight.StopCoroutine(DayNightSystemExtension.weatherCoroutine);

            int currentDay = dayNight.previewDay;
            int currentTime = dayNight.previewTime;

            if (duration <= 0)
            {
                if (mode == TimeAndWeatherOption.Reset)
                {
                    if (sceneDefaultWeather.TryGetValue(sceneName, out Weather defaultWeather))
                    {
                        if (!sun) sceneSuns.TryGetValue(sceneName, out sun);
                        dayNight.SetWeatherOverride(defaultWeather, sun);
                    }
                    else dayNight.UpdateDayTimeSettings(true);
                }
                else
                {
                    if (!sun) sceneSuns.TryGetValue(sceneName, out sun);
                    dayNight.SetWeatherOverride(mode.ToString(), sun);
                }
            }
            else
            {
                string newWeather = mode.ToString();
                string prevWeather = currentTimeAndWeather.ToString();

                dayNight.GetDayTime(true, out int day, out int time);
                if (mode == TimeAndWeatherOption.Reset) newWeather = dayNight.days[day].timeSlots[time].weather;
                if (currentTimeAndWeather == TimeAndWeatherOption.Reset) prevWeather = dayNight.days[day].timeSlots[time].weather;

                if (!sun) sceneSuns.TryGetValue(sceneName, out sun);
                dayNight.SetWeatherOverrideLerp(newWeather, prevWeather, duration, sun);
            }

            dayNight.previewDay = currentDay;
            dayNight.previewTime = currentTime;
        }


        private static void SetCloudRotation(float rotation, float duration)
        {
            Scene activeScene = SceneManager.GetActiveScene();
            string sceneName = activeScene.name;

            // If there is a DayNightSystem in the current scene, only use that
            DayNightSystem dayNight = dayNightSystems.FirstOrDefault(dayNight => dayNight.gameObject.scene == activeScene && dayNight.isActiveAndEnabled) ?? masterDayNightSystem;
            if (dayNight)
            {
                if (DayNightSystemExtension.cloudRotationCoroutine != null) dayNight.StopCoroutine(DayNightSystemExtension.cloudRotationCoroutine);

                if (duration > 0) dayNight.SetCloudRotationLerp(currentCloudRotation, rotation, duration);
                else dayNight.SetCloudRotation(rotation);
            }

            currentCloudRotation = rotation;
        }
    }
}