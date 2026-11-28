using System.Linq;
using FMODUnity;
using Rhythm;
using TMPro;
using UnityEngine;

namespace UNANIMATED.Countdown
{
    public static class CountdownController
    {
        public static readonly int countdownOffset = -35;

        public static bool disableCountdown;
        public static bool customCountdownEnabled;
        public static bool playedTick;
        public static bool requestingCountdown;
        public static bool requestingText;

        public static int countdownEnd;
        public static float countdownLength;
        public static float countdownBeatLength;
        public static int[] countdownBeats;
        public static int currentCountdownBeat;
        public static string text;

        public static EventReference countdownTick;


        public static void Reset()
        {
            disableCountdown = false;
            customCountdownEnabled = false;
            requestingCountdown = false;
            playedTick = false;
            countdownBeatLength = 0;
            currentCountdownBeat = 0;
            countdownEnd = 0;
            countdownLength = 0;
            countdownBeats = [];
            countdownTick = default;
        }

        public static void AddCountdown(CommandEventInfo currentCommand)
        {
            if (disableCountdown) return;

            // Get values
            countdownEnd = currentCommand.GetIntParam(2);
            countdownBeatLength = currentCommand.GetFloatParam(3);
            countdownBeats = [.. currentCommand.GetIntArrayParam(4).Reverse()];
            countdownLength = currentCommand.GetFloatParam(1) + countdownBeats.Length * countdownBeatLength;
            currentCountdownBeat = countdownBeats.Length;

            // Logging
            UNANIMATED.Logger.LogInfo($"Parsed countdown command: params {currentCommand.ParamString}");
            if (countdownBeats.Length == 0) UNANIMATED.Logger.LogWarning("No countdown beats were given or the array failed to parse, double check your countdown command.");
            else if (countdownBeats.All(i => i == 0)) UNANIMATED.Logger.LogWarning("All countdown beats are 0 or the array failed to parse, double check your countdown command.");

            customCountdownEnabled = true;
        }

        public static void ParseCommand(CommandEventInfo currentCommand)
        {
            // Get values
            CountdownOption type = currentCommand.GetEnumParamForced<CountdownOption>(0);
            currentCountdownBeat = currentCommand.GetIntParam(1);
            float time = currentCommand.Duration;

            // Logging
            UNANIMATED.Logger.LogInfo($"Parsed countdown command at {currentCommand.Time} ms: {type} | params {currentCommand.ParamString} | length {currentCommand.Duration:0} ms");

            // Enable custom countdowns
            customCountdownEnabled = true;

            // Disable the other code so we don't override it
            disableCountdown = true;

            // Set the parameters
            if (type == CountdownOption.Tick && currentCountdownBeat > 0)
            {
                PlayCountdownTick();
                playedTick = false;
            }
            else if (type == CountdownOption.Count || type == CountdownOption.Display)
            {
                if (time == 0) countdownBeatLength = float.MaxValue;
                else countdownBeatLength = time;
                requestingCountdown = true;
                playedTick = false;
            }
            else if (type == CountdownOption.Text)
            {
                if (time == 0) countdownBeatLength = float.MaxValue;
                else countdownBeatLength = time;
                requestingText = true;
                text = currentCommand.GetStringParam(1);
            }
        }


        public static void PlayCountdownTick()
        {
            if (!countdownTick.IsNull && !playedTick && !requestingCountdown)
            {
                RuntimeManager.PlayOneShot(countdownTick, default(Vector3));
                playedTick = true;
            }
        }


        public static void DisplayCountdownBeat(TextMeshProUGUI display, int beat)
        {
            switch (beat)
            {
                case 0:
                    break;
                case 1:
                    display.text = "ONE";
                    break;
                case 2:
                    display.text = "TWO";
                    break;
                case 3:
                    display.text = "THREE";
                    break;
                case 4:
                    display.text = "FOUR";
                    break;
                case 5:
                    display.text = "FIVE";
                    break;
                case 6:
                    display.text = "SIX";
                    break;
                case 7:
                    display.text = "SEVEN";
                    break;
                case 8:
                    display.text = "EIGHT";
                    break;
                case 9:
                    display.text = "NINE";
                    break;
                case 10:
                    display.text = "TEN";
                    break;
                case 11:
                    display.text = "ELEVEN";
                    break;
                case 12:
                    display.text = "TWELVE";
                    break;
                case 13:
                    display.text = "THIRTEEN";
                    break;
                case 14:
                    display.text = "FOURTEEN";
                    break;
                case 15:
                    display.text = "FIFTEEN";
                    break;
                case 16:
                    display.text = "SIXTEEN";
                    break;
                default:
                    display.text = string.Empty;
                    break;
            }

            if (beat > 0)
            {
                int random = beat % 2 == 0 ? Random.Range(-3, 1) : Random.Range(0, 4);
                display.transform.Rotate(new Vector3(0f, 0f, random));
            }
        }
    }
}