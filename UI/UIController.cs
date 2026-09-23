using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Rhythm;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UNANIMATED.UI
{
    public static class UIController
    {
        // GameObjects
        // public static Graphic score;
        // public static Graphic[] accuracy;
        // public static Graphic maxCombo;
        // public static Graphic[] vignetteBars;
        // public static Graphic[] blackBGBars;
        // public static Graphic[] fourByThreeBars;
        // public static Graphic health;
        // public static Graphic[] JudgementLine;
        // public static Graphic judgements;
        // public static GameObject measureBars;
        // public static Graphic speedLines;
        // public static Graphic upNextIndicators;
        // public static Graphic reticle;
        // public static Graphic[] notes;
        public static Dictionary<UIType, List<Renderer>> uiElementRenderers = [];
        public static Dictionary<UIType, List<GameObject>> uiElementObjects = [];
        public static Dictionary<UIType, List<Graphic>> uiElementGraphics = [];
        public static Dictionary<SlideUIType, RhythmGameSlideOutUI> uiSlideElements = [];
        public static Dictionary<SlideUIType, List<ScaleOnSongStart>> uiScaleElements = [];


        public static ToggleFourByThree toggleFourByThree;
        public static Transform measureBarsParent;


        // States
        public static bool forceLockedUI;

        public static void Init(RhythmController controller)
        {
            Transform trans = controller.transform.parent;

            // Score
            uiElementGraphics.TryAdd(UIType.Score, [trans.GetComponentInChildren<RhythmScoreDisplay>(true)?.GetComponent<TextMeshProUGUI>()]);

            // Accuracy
            uiElementGraphics.TryAdd(UIType.Accuracy, [trans.GetComponentInChildren<RhythmComboDisplay>(true)?.GetComponent<TextMeshProUGUI>(), trans.GetComponentInChildren<RhythmAccuracyDisplay>()?.GetComponent<TextMeshProUGUI>()]);

            // Max combo
            uiElementGraphics.TryAdd(UIType.MaxCombo, [trans.GetComponentInChildren<RhythmFullComboDisplay>(true)?.GetComponent<TextMeshProUGUI>()]);

            // Vignette
            List<Graphic> vignettes = [];
            vignettes.Add(trans.Find("RhythmUI/UiParentCanvas/UiParent/ScorecardBackPink")?.GetComponent<Image>());
            vignettes.Add(trans.Find("RhythmUI/UiParentCanvas/UiParent/ScorecardBackPink (1)")?.GetComponent<Image>());
            vignettes.Add(trans.Find("RhythmUI/UiParentCanvas/UiParent/ScorecardBackPink (2)")?.GetComponent<Image>());
            vignettes.Add(trans.Find("RhythmUI/UiParentCanvas/UiParent/ScorecardBackPink (3)")?.GetComponent<Image>());
            vignettes.Add(trans.Find("RhythmUI/UiParentCanvas/UiParent/Vignette")?.GetComponent<Image>());
            vignettes.Add(trans.Find("RhythmUI/UiParentCanvas/UiParent/Vignette/VignetteSpeedLines")?.GetComponent<Image>());
            vignettes.Add(trans.Find("RhythmUI/UiParentCanvas/UiParent/Vignette/VignetteFillTop")?.GetComponent<Image>());
            vignettes.Add(trans.Find("RhythmUI/UiParentCanvas/UiParent/Vignette/VignetteFillBot")?.GetComponent<Image>());
            vignettes.Add(trans.Find("RhythmUI/UiParentCanvas/UiParent/Vignette (1)")?.GetComponent<Image>());
            vignettes.Add(trans.Find("RhythmUI/UiParentCanvas/UiParent/Vignette (1)/VignetteFillTop")?.GetComponent<Image>());
            vignettes.Add(trans.Find("RhythmUI/UiParentCanvas/UiParent/Vignette (1)/VignetteFillBot")?.GetComponent<Image>());
            uiElementGraphics.TryAdd(UIType.VignetteBars, vignettes);

            // Black BG bars
            RotationJitter[] bars = trans.GetComponentsInChildren<RotationJitter>();
            uiElementRenderers.TryAdd(UIType.BlackBGBars, bars?.Select(e => e.GetComponent<Renderer>()).ToList());
            uiScaleElements.TryAdd(SlideUIType.BlackBGBars, bars?.Select(e => e.GetComponent<ScaleOnSongStart>()).ToList());

            // 4x3
            toggleFourByThree = trans.GetComponentInChildren<ToggleFourByThree>();

            // Health
            uiElementObjects.TryAdd(UIType.Health, [trans.GetComponentInChildren<RhythmHealthDisplay>(true)?.gameObject]);

            // Judgement line
            // List<Renderer> judgementLine = [];
            // judgementLine.AddRange(trans.Find("Left Guide")?.GetComponentsInChildren<Renderer>());
            // judgementLine.AddRange(trans.Find("Right Guide")?.GetComponentsInChildren<Renderer>());
            uiElementRenderers.TryAdd(UIType.LeftJudgementLine, trans.Find("Left Guide")?.GetComponentsInChildren<Renderer>().ToList());
            uiElementRenderers.TryAdd(UIType.RightJudgementLine, trans.Find("Right Guide")?.GetComponentsInChildren<Renderer>().ToList());

            // Measure bars
            GameObject measureBarsParentObject = new("MeasureBars");
            measureBarsParentObject.transform.parent = controller.transform;
            measureBarsParent = measureBarsParentObject.transform;
            uiElementObjects.TryAdd(UIType.MeasureBars, [measureBarsParentObject]);

            // Speed lines
            uiElementGraphics.TryAdd(UIType.SpeedLines, [trans.Find("RhythmUI/DrawMaskCanvas?/BackgroundSpeedLines")?.GetComponent<Image>()]);

            // Reticle
            uiElementObjects.TryAdd(UIType.Reticle, [trans.GetComponentInChildren<ToggleReticle>()?.gameObject]);

            // Slide UI
            RhythmGameSlideOutUI[] slideElements = trans.GetComponentsInChildren<RhythmGameSlideOutUI>();
            foreach (RhythmGameSlideOutUI elem in slideElements)
            {
                if (elem.name == "UiParentCanvas") uiSlideElements.TryAdd(SlideUIType.AllOverlayUI, elem);
                if (elem.name == "Right Guide") uiSlideElements.TryAdd(SlideUIType.RightJudgementLine, elem);
                if (elem.name == "Left Guide") uiSlideElements.TryAdd(SlideUIType.LeftJudgementLine, elem);
                if (elem.name == "BackgroundFade") uiSlideElements.TryAdd(SlideUIType.BackgroundGradient, elem);
                if (elem.name == "HealthBar") uiSlideElements.TryAdd(SlideUIType.HealthBar, elem);
                if (elem.name == "BackgroundSpeedLines") uiSlideElements.TryAdd(SlideUIType.SpeedLines, elem);
            }

            // Up next
            uiElementObjects.TryAdd(UIType.UpNextIndicators, [controller.upNextIndicatorLeft.gameObject, controller.upNextIndicatorRight.gameObject]);


            // Notes
            uiElementObjects.TryAdd(UIType.Notes, [controller.noteGroup]);
        }

        public static void Reset()
        {
            forceLockedUI = false;
            uiElementGraphics = [];
            uiElementObjects = [];
            uiElementRenderers = [];
            uiSlideElements = [];
            uiScaleElements = [];
            toggleFourByThree = null;
            measureBarsParent = null;
        }

        public static void ParseCommand(CommandEventInfo currentCommand)
        {
            // Get relevant variables
            UIOption type = currentCommand.GetEnumParamForced<UIOption>(0);
            bool enabled = currentCommand.GetBoolParam(1, true);
            Enum.TryParse(currentCommand.GetStringParam(1).Replace('|', ','), out UIType elements);
            Enum.TryParse(currentCommand.GetStringParam(1).Replace('|', ','), out SlideUIType slideElements);

            float time = currentCommand.Duration / 1000f;

            // Logging
            UNANIMATED.Logger.LogInfo($"Parsed UI command at {currentCommand.Time} ms: {type} | params {currentCommand.ParamString} | length {time:0} ms");

            // Command logic
            switch (type)
            {
                case UIOption.Reset:
                    Reset();
                    break;
                case UIOption.ForceLockedUI:
                    forceLockedUI = enabled;
                    break;

                case UIOption.Show:
                    SetUIState(elements, time, true);
                    break;

                case UIOption.Hide:
                    SetUIState(elements, time, false);
                    break;

                case UIOption.SlideIn:
                    SetUISlideState(slideElements, true);
                    break;

                case UIOption.SlideOut:
                    SetUISlideState(slideElements, false);
                    break;
            }
        }


        private static void SetUIState(UIType elements, float time, bool enabled)
        {
            foreach (UIType uiType in Enum.GetValues(typeof(UIType)))
            {
                if (elements.HasFlag(uiType))
                {
                    // Disable any renderers that are supposed to be hidden
                    if (uiElementRenderers.TryGetValue(uiType, out List<Renderer> renderers)) renderers.ForEach(rend => { if (rend != null) rend.enabled = enabled; });

                    // Disable any GameObjects that are supposed to be disabled
                    if (uiElementObjects.TryGetValue(uiType, out List<GameObject> objects)) objects.ForEach(obj => obj?.SetActive(enabled));

                    // Tween the alpha of any graphics
                    if (uiElementGraphics.TryGetValue(uiType, out List<Graphic> graphics)) graphics.ForEach(g => DOFade(g, enabled ? 1 : 0, time));

                    // Special cases
                    if (uiType == UIType.FourByThreeBars)
                    {
                        toggleFourByThree.animator.Play(enabled ? "4x3TransitionIn" : "4x3TransitionOut", -1, 0.99f);
                    }
                }
            }
        }


        private static void SetUISlideState(SlideUIType elements, bool enabled)
        {
            foreach (SlideUIType uiType in Enum.GetValues(typeof(SlideUIType)))
            {
                if (elements.HasFlag(uiType))
                {
                    if (uiSlideElements.TryGetValue(uiType, out RhythmGameSlideOutUI slideUI))
                    {
                        slideUI._startedIDK = true;

                        bool original = slideUI.rhythmController.hideUIOnStart;
                        slideUI.rhythmController.hideUIOnStart = true;

                        if (enabled) slideUI.OnPlay();
                        else slideUI.Complete(true);

                        slideUI.rhythmController.hideUIOnStart = original;
                    }

                    if (uiScaleElements.TryGetValue(uiType, out List<ScaleOnSongStart> scaleUIs) && scaleUIs != null)
                    {
                        scaleUIs.ForEach(scale => scale.delayTimer = enabled ? 20 : 10);
                    }

                    // Special cases
                    if (uiType == SlideUIType.FourByThreeBars)
                    {
                        toggleFourByThree.useFourByThree = enabled;
                    }
                }
            }
        }



        private static void DOFade(Graphic target, float endValue, float time)
        {
            TweenerCore<Color, Color, ColorOptions> tweenerCore = DOTween.ToAlpha(() => target.color, delegate (Color x) { target.color = x; }, endValue, time);
            tweenerCore.SetTarget(target);
        }


        public static class Defaults
        {
            public static readonly float canvasCamFollowerScale = 0.1069f;
        }
    }
}