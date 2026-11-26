using System.Linq;
using FMODUnity;
using HarmonyLib;
using Rhythm;
using UnityEngine;

namespace UNANIMATED.Countdown
{
    [HarmonyPatch(typeof(RhythmController))]
    [HarmonyPatch(nameof(RhythmController.InitializeAndPlay))]
    // [HarmonyPriority(Priority.First)]
    internal class ControllerInitializeAndPlayPatch
    {
        static bool Prefix(ref RhythmController __instance, ref bool countdownOn)
        {
            if (CountdownController.disableCountdown)
            {
                // Disable base game countdown
                countdownOn = false;

                // Add countdown that doesn't do anything
                __instance.songTracker.AddCountdown(0, 0, 0);
            }
            else if (CountdownController.customCountdownEnabled)
            {
                // Disable base game countdown
                countdownOn = false;

                // Add countdown for mods that depend on the call
                __instance.songTracker.AddCountdown(CountdownController.countdownLength, CountdownController.countdownEnd, CountdownController.countdownBeatLength);

                // Add custom countdown
                __instance.songTracker.countdown = null;
                __instance.song.countdownBeatLength = CountdownController.countdownBeatLength;
                CountdownController.countdownTick = __instance.countdownStick;
            }

            return true;
        }
    }



    [HarmonyPatch(typeof(RhythmTracker))]
    [HarmonyPatch(nameof(RhythmTracker.FixedUpdate))]
    internal class TrackerFixedUpdatePatch
    {
        static void Postfix(ref RhythmTracker __instance)
        {
            if (CountdownController.disableCountdown)
            {
                __instance.countdown = null;
                __instance.countdownBeat = 0;
            }
            else if (CountdownController.customCountdownEnabled)
            {
                float length = CountdownController.countdownLength;
                float beatLength = CountdownController.countdownBeatLength;
                float countdownPosition = __instance.currentPosition + length - CountdownController.countdownEnd + CountdownController.countdownOffset;

                if (CountdownController.currentCountdownBeat >= 0 && Mathf.CeilToInt((length - countdownPosition) / beatLength) <= CountdownController.currentCountdownBeat)
                {
                    CountdownController.currentCountdownBeat--;

                    int currentBeatMode = CountdownController.countdownBeats.ElementAtOrDefault(CountdownController.currentCountdownBeat);
                    if (currentBeatMode > 0) RuntimeManager.PlayOneShot(__instance.tickEvent, default(Vector3));
                }

                __instance.countdown = null;
                __instance.countdownBeat = CountdownController.currentCountdownBeat;
            }
        }
    }



    [HarmonyPatch(typeof(RhythmCountdown))]
    [HarmonyPatch(nameof(RhythmCountdown.Update))]
    internal class CountdownUpdatePatch
    {
        static bool Prefix(ref RhythmCountdown __instance)
        {
            if (CountdownController.customCountdownEnabled && __instance.display)
            {
                if (!__instance.deactivated && __instance.prevPhase != CountdownController.currentCountdownBeat)
                {
                    __instance.prevPhase = CountdownController.currentCountdownBeat;
                    __instance.deactivateTimer = CountdownController.countdownBeatLength;

                    CountdownController.DisplayCountdownBeat(__instance.display, CountdownController.countdownBeats.ElementAtOrDefault(CountdownController.currentCountdownBeat));

                    if (CountdownController.currentCountdownBeat <= 0)
                    {
                        __instance.deactivated = true;
                        CountdownController.disableCountdown = false;
                    }
                }
                else if (CountdownController.requestingCountdown)
                {
                    CountdownController.requestingCountdown = false;
                    __instance.deactivateTimer = CountdownController.countdownBeatLength;
                    CountdownController.DisplayCountdownBeat(__instance.display, CountdownController.currentCountdownBeat);
                    CountdownController.playedTick = false;
                }
                else if (CountdownController.requestingText)
                {
                    CountdownController.requestingText = false;
                    __instance.deactivateTimer = CountdownController.countdownBeatLength;
                    __instance.display.text = CountdownController.text;
                }

                if (__instance.deactivateTimer > 0f) __instance.deactivateTimer -= Time.deltaTime * 1000f;
                else if (__instance.deactivated && __instance.display.text != string.Empty) __instance.display.text = string.Empty;

                return false;
            }
            else return true;
        }
    }
}