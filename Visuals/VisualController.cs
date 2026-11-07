using System.Collections.Generic;
using System.Linq;
using Overworld;
using Rhythm;
using UnityEngine;

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

        // Other
        public static List<DayNightSystem> dayNightSystems = [];

        // States
        public static bool isInverted;
        public static TimeAndWeatherOption currentTimeAndWeather;

        public static void Init(RhythmController controller)
        {
            Reset();
        }


        public static void GetDayNightSystems()
        {
            dayNightSystems = Object.FindObjectsByType<DayNightSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None).Cast<DayNightSystem>().ToList();

            foreach (DayNightSystem dayNight in dayNightSystems)
            {
                dayNight.RefreshOnEnable = false;
                dayNight.enabled = true;
                dayNight.UpdateDayTimeSettings(true);
                // dayNight.previousDay = dayNight.currentDay;
                // dayNight.previousTime = dayNight.currentTime;

                // // Set the default weather with the existing preview day and time
                // dayNight.GetDayTime(true, out var day, out var time);
                // Day customDay = new()
                // {
                //     timeSlots = [new TimeSlots() {
                //         weather = dayNight.days[day].timeSlots[time].weather,
                //         dependentObjects = [null]
                //     }]
                // };

                // dayNight.days.Add(customDay);
            }
        }


        public static void Reset()
        {
            isInverted = false;
            dayNightSystems = [];
        }

        public static void ParseCommand(CommandEventInfo currentCommand)
        {
            // Get relevant variables
            VisualOption type = currentCommand.GetEnumParamForced<VisualOption>(0);
            // bool enabled = currentCommand.GetBoolParam(1);
            float time = currentCommand.Duration / 1000f;

            // Logging
            UNANIMATED.Logger.LogInfo($"Parsed visuals command at {currentCommand.Time} ms: {type} | params {currentCommand.ParamString} | length {time:0} ms");

            // Command logic
            switch (type)
            {
                case VisualOption.Reset:
                    Reset();
                    break;
                case VisualOption.TimeAndWeather:
                    SetTimeAndWeather(currentCommand.GetEnumParamForced<TimeAndWeatherOption>(1), time);
                    break;


                    // case VisualOption.Invert:

                    //     break;

                    // default:
                    //     {
                    //         // Other mode
                    //         if (time >= 0)
                    //         {
                    //             // Command has a time
                    //             switch (type)
                    //             {

                    //             }

                    //         }
                    //         else
                    //         {
                    //             // Command is instant
                    //             switch (type)
                    //             {

                    //             }
                    //         }
                    //     }
                    //     break;
            }
        }

        private static void SetTimeAndWeather(TimeAndWeatherOption mode, float duration)
        {
            foreach (DayNightSystem dayNight in dayNightSystems)
            {
                dayNight.StopAllCoroutines();

                int currentDay = dayNight.previewDay;
                int currentTime = dayNight.previewTime;

                if (duration <= 0)
                {
                    if (mode != TimeAndWeatherOption.Reset)
                    {
                        dayNight.SetWeatherOverride(mode.ToString());
                    }
                    else
                    {
                        dayNight.UpdateDayTimeSettings(true);
                    }
                }
                else
                {
                    string newWeather = mode.ToString();
                    string prevWeather = currentTimeAndWeather.ToString();

                    dayNight.GetDayTime(true, out int day, out int time);
                    if (mode == TimeAndWeatherOption.Reset) newWeather = dayNight.days[day].timeSlots[time].weather;
                    if (currentTimeAndWeather == TimeAndWeatherOption.Reset) prevWeather = dayNight.days[day].timeSlots[time].weather;

                    dayNight.SetWeatherOverrideLerp(newWeather, prevWeather, duration);
                }


                // // Remove old custom weather
                // dayNight.days.RemoveAt(dayNight.days.Count() - 1);

                // Day customDay;

                // if (mode != TimeAndWeatherOption.Reset)
                // {
                //     // Create custom day with desired weather
                //     customDay = new()
                //     {
                //         timeSlots = [new TimeSlots() {
                //             weather = mode.ToString(),
                //             dependentObjects = [null]
                //         }]
                //     };

                //     dayNight.previewDay = dayNight.days.Count() - 1;
                //     dayNight.previewTime = 0;
                // }
                // else
                // {
                //     // Set the default weather with the existing preview day and time
                //     dayNight.GetDayTime(true, out var day, out var time);
                //     customDay = new()
                //     {
                //         timeSlots = [new TimeSlots() {
                //         weather = dayNight.days[day].timeSlots[time].weather,
                //         dependentObjects = [null]
                //     }]
                //     };
                // }

                // // Add custom day
                // dayNight.days.Add(customDay);


                // dayNight.previousDay = dayNight.currentDay;
                // dayNight.previousTime = dayNight.currentTime;

                dayNight.previewDay = currentDay;
                dayNight.previewTime = currentTime;

                currentTimeAndWeather = mode;
            }
        }
    }
}