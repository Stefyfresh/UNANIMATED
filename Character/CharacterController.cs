using System;
using System.Collections.Generic;
using System.Linq;
using Arcade.Unlockables;
using DG.Tweening;
using Rhythm;
using UnityEngine;

namespace UNANIMATED.Character
{
    public static class CharacterController
    {
        public static RhythmCharacterSelector spawner;
        public static string customPrimaryCharacter;
        public static string customSecondaryCharacter;

        public static Dictionary<BGCharacter, Color> bgCharacterColours = [];
        public static Color primaryCharacterColour = Color.white;
        public static bool disableAssist;

        public static Tweener primaryTweener;
        public static Dictionary<BGCharacter, Tweener> bgTweeners = [];


        public static void Init(RhythmController controller)
        {
            GameObject charSpawnerGO = GameObject.Find("Arcade Character Spawner");
            Reset();
            spawner = charSpawnerGO.GetComponent<RhythmCharacterSelector>();
            bgCharacterColours.Add(BGCharacter.Left, Color.white);
            bgCharacterColours.Add(BGCharacter.LeftMiddle, Color.white);
            bgCharacterColours.Add(BGCharacter.Middle, Color.white);
            bgCharacterColours.Add(BGCharacter.RightMiddle, Color.white);
            bgCharacterColours.Add(BGCharacter.Right, Color.white);
        }


        public static void ManualUpdate(RhythmController controller)
        {
            SpriteRenderer renderer;
            Color colour;

            // Set colours
            renderer = spawner.BGCharL.GetComponentInChildren<SpriteRenderer>();
            if (renderer && bgCharacterColours.TryGetValue(BGCharacter.Left, out colour)) renderer.color = colour;

            renderer = spawner.BGCharML.GetComponentInChildren<SpriteRenderer>();
            if (renderer && bgCharacterColours.TryGetValue(BGCharacter.LeftMiddle, out colour)) renderer.color = colour;

            renderer = spawner.BGCharM.GetComponentInChildren<SpriteRenderer>();
            if (renderer && bgCharacterColours.TryGetValue(BGCharacter.Middle, out colour)) renderer.color = colour;

            renderer = spawner.BGCharMR.GetComponentInChildren<SpriteRenderer>();
            if (renderer && bgCharacterColours.TryGetValue(BGCharacter.RightMiddle, out colour)) renderer.color = colour;

            renderer = spawner.BGCharR.GetComponentInChildren<SpriteRenderer>();
            if (renderer && bgCharacterColours.TryGetValue(BGCharacter.Right, out colour)) renderer.color = colour;


            controller.player.assist.sprite.enabled = !disableAssist;


            controller.player.sprite.color = primaryCharacterColour;
            TrailRenderer trail = controller.player.GetComponentInChildren<TrailRenderer>();
            if (trail)
            {
                Gradient gradient = trail.colorGradient;

                GradientAlphaKey[] keys = gradient.alphaKeys;
                keys[0] = new GradientAlphaKey(0, keys[0].time);
                keys[1] = new GradientAlphaKey(primaryCharacterColour.a, keys[1].time);
                keys[2] = new GradientAlphaKey(primaryCharacterColour.a, keys[2].time);
                gradient.alphaKeys = keys;

                trail.colorGradient = gradient;
            }
        }



        public static void Reset()
        {
            bgTweeners.Values.ToList().ForEach(tweener => tweener.Kill());
            primaryTweener.Kill();

            customPrimaryCharacter = FileStorage.beatmapOptions.primaryCharacter;
            customSecondaryCharacter = FileStorage.beatmapOptions.primaryCharacter;
            primaryCharacterColour = Color.white;
            bgCharacterColours = [];
            bgTweeners = [];
            primaryTweener = null;
            disableAssist = false;
            spawner = null;
        }



        public static void ParseCommand(CommandEventInfo currentCommand)
        {
            // Get relevant variables
            CharacterOption type = currentCommand.GetEnumParamForced<CharacterOption>(0);
            float time = currentCommand.Duration / 1000f;

            // Logging
            UNANIMATED.Logger.LogInfo($"Parsed Character command at {currentCommand.Time} ms: {type} | params {string.Join(", ", currentCommand.Parameters)} | length {currentCommand.Duration:0} ms");


            switch (type)
            {
                case CharacterOption.Reset:
                    Reset();
                    ReplacePrimaryCharacter();
                    ReplaceSecondaryCharacter();
                    ReplaceBackgroundCharacters();
                    break;
                case CharacterOption.SetCharacter:
                    customPrimaryCharacter = currentCommand.GetStringParam(1);
                    if (customPrimaryCharacter == "Reset") customPrimaryCharacter = FileStorage.beatmapOptions.primaryCharacter;
                    ReplacePrimaryCharacter();

                    customSecondaryCharacter = currentCommand.GetStringParam(2);
                    if (customSecondaryCharacter == "Reset") customSecondaryCharacter = FileStorage.beatmapOptions.secondaryCharacter;
                    ReplaceSecondaryCharacter();

                    ReplaceBackgroundCharacters();
                    break;
                case CharacterOption.ShowAssist:
                    disableAssist = !currentCommand.GetBoolParam(1, true);
                    break;
                case CharacterOption.SetOpacity:
                    float opacity = currentCommand.GetFloatParam(1);
                    if (primaryTweener != null && primaryTweener.IsActive()) primaryTweener.Kill();
                    if (time > 0) primaryTweener = DOTween.ToAlpha(() => primaryCharacterColour, delegate (Color x) { primaryCharacterColour = x; }, opacity, time).SetEase(Ease.OutQuad).SetAutoKill(true);
                    else primaryCharacterColour = new Color(primaryCharacterColour.r, primaryCharacterColour.g, primaryCharacterColour.b, opacity);
                    break;
                case CharacterOption.SetBGOpacity:
                    SetBackgroundCharacterOpacity(currentCommand.GetFlagsEnumParam<BGCharacter>(2), currentCommand.GetFloatParam(1), time);
                    break;
            }
        }




        public static void ReplacePrimaryCharacter()
        {
            if (string.IsNullOrWhiteSpace(customPrimaryCharacter)) customPrimaryCharacter = FileStorage.beatmapOptions.primaryCharacter;


            if (spawner.index.TryGetCharacter(customPrimaryCharacter, out CharacterIndex.Character character2))
            {
                GameObject newCharacterGO = UnityEngine.Object.Instantiate(character2.prefab, spawner.player.transform);
                RhythmPlayerAnimator currentAnimator = spawner.player.currentAnimator;
                spawner.player.SetCurrentAnimator(newCharacterGO.GetComponent<RhythmPlayerAnimator>());

                if (currentAnimator) UnityEngine.Object.Destroy(currentAnimator.gameObject);

            }
            else
            {
                UNANIMATED.Logger.LogWarning($"[Character] Could not set assist character \"{customPrimaryCharacter}\" as it does not exist!");
            }
        }



        public static void ReplaceSecondaryCharacter()
        {
            if (string.IsNullOrWhiteSpace(customSecondaryCharacter)) customSecondaryCharacter = FileStorage.beatmapOptions.secondaryCharacter;

            if (spawner.index.TryGetCharacter(customSecondaryCharacter, out CharacterIndex.Character character2))
            {
                GameObject newCharacterGO = UnityEngine.Object.Instantiate(character2.assistPrefab, spawner.assist.transform);
                RhythmAssistAnimator currentAnimator = spawner.assist.currentAnimator;
                spawner.assist.currentAnimator = newCharacterGO.GetComponent<RhythmAssistAnimator>();
                spawner.assist.GetComponents();

                if (currentAnimator) UnityEngine.Object.Destroy(currentAnimator.gameObject);

                // if (spawner.assist.atRest)
                // {
                spawner.assist.sprite.color = currentAnimator._sprite.color;
                // }
            }
            else
            {
                UNANIMATED.Logger.LogWarning($"[Character] Could not set assist character \"{customSecondaryCharacter}\" as it does not exist!");
            }
        }



        public static void ReplaceBackgroundCharacters()
        {
            Utils.DestroyChildren(spawner.BGCharL);
            Utils.DestroyChildren(spawner.BGCharM);
            Utils.DestroyChildren(spawner.BGCharML);
            Utils.DestroyChildren(spawner.BGCharMR);
            Utils.DestroyChildren(spawner.BGCharR);

            spawner.InstantiateBG(customPrimaryCharacter, customSecondaryCharacter);
        }



        public static void SetBackgroundCharacterOpacity(BGCharacter characters, float opacity, float time)
        {
            if (characters == default)
            {
                UNANIMATED.Logger.LogWarning("No valid background characters were selected, defaulting to all. Double check the param string or specify All to target all elements at once.");
                characters = BGCharacter.All;
            }

            foreach (BGCharacter character in Enum.GetValues(typeof(BGCharacter)))
            {
                if (characters.HasFlag(character))
                {
                    if (bgCharacterColours.TryGetValue(character, out Color colour))
                    {
                        // Kill old tweener
                        if (bgTweeners.TryGetValue(character, out Tweener tweener))
                        {
                            if (tweener != null & tweener.IsActive()) tweener.Kill();
                            bgTweeners.Remove(character);
                        }

                        if (time > 0) bgTweeners.Add(character, DOTween.ToAlpha(() => bgCharacterColours[character], delegate (Color x) { bgCharacterColours[character] = x; }, opacity, time).SetEase(Ease.OutQuad).SetAutoKill(true));
                        else bgCharacterColours[character] = new Color(colour.r, colour.g, colour.b, opacity);
                    }
                }
            }
        }
    }
}