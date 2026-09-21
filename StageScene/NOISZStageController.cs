using System;
using System.Linq;
using Rhythm;
using UnityEngine;
using UnityEngine.Video;
using DG.Tweening;
using UnityEngine.UI;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;

namespace UNANIMATED.StageScene
{
    public static class NOISZStageController
    {
        // Constants
        public static readonly float transitionShowAlpha = 0.992f;
        public static readonly float transitionHideAlpha = 0;

        // Prefabs
        private static DodgeNote normalSpikePrefab;
        private static DodgeNote noiszSpikePrefab;

        // Other things
        private static Transform normalMasksParent;
        private static GameObject noiszBGObject;
        private static Image noiszBGImage;

        // States
        public static bool succeededPreloading;
        public static bool noiszModeOn;


        public static void Init(RhythmController controller)
        {
            try
            {
                if (controller != null && UNANIMATED.enableSceneSwitching.Value)
                {
                    normalSpikePrefab = controller.notePrefabGroups[0].dodgeNotePrefab;
                    normalMasksParent = controller.transform.parent.GetComponentInChildren<RhythmStencilMasks>()?.transform;
                    succeededPreloading = true;
                }
            }
            catch (Exception ex)
            {
                UNANIMATED.Logger.LogWarning($"Failed NOISZ stage initialization! {ex.Message}");
                UNANIMATED.Logger.LogWarning(ex.StackTrace);
                succeededPreloading = false;
            }
        }

        public static void ManualUpdate(RhythmController controller)
        {
            try
            {
                if (succeededPreloading)
                {
                    if (!noiszModeOn && !controller.song.over && controller.activeNotes != null && controller.activeNotes.Any(note => note is DodgeNote))
                    {
                        noiszModeOn = true;
                        DOTransition(transitionShowAlpha);
                    }
                    if (controller.song.over && noiszModeOn)
                    {
                        noiszModeOn = false;
                        DOTransition(transitionHideAlpha);
                    }
                }
            }
            catch (Exception ex)
            {
                UNANIMATED.Logger.LogWarning($"Failed NOISZ stage background switch! {ex.Message}");
                UNANIMATED.Logger.LogWarning(ex.StackTrace);
            }
        }


        public static void SpecialPreloadNOISZStage(GameObject controllerRoot)
        {
            try
            {
                if (UNANIMATED.enableSceneSwitching.Value && succeededPreloading)
                {
                    noiszSpikePrefab = controllerRoot.GetComponentInChildren<RhythmController>().notePrefabGroups[0].dodgeNotePrefab;
                    GameObject noiszMask = controllerRoot.GetComponentInChildren<NOISZBGController>().transform.Find("BackgroundFill").gameObject;

                    noiszBGObject = UnityEngine.Object.Instantiate(noiszMask, normalMasksParent);
                    noiszBGObject.name = "NOISZ_BG";
                    noiszBGObject.transform.localPosition = noiszMask.transform.localPosition;
                    noiszBGObject.transform.localEulerAngles = noiszMask.transform.localEulerAngles;
                    noiszBGObject.transform.localScale = noiszMask.transform.localScale;

                    noiszBGImage = noiszBGObject.GetComponent<Image>();

                    UNANIMATED.Logger.LogInfo("Finished NOISZ stage special preloading.");
                    succeededPreloading = true;
                }
            }
            catch (Exception ex)
            {
                UNANIMATED.Logger.LogWarning($"Failed NOISZ stage special preloading! {ex.Message}");
                UNANIMATED.Logger.LogWarning(ex.StackTrace);
                succeededPreloading = false;
            }
        }


        public static void SwitchNOISZ(bool sceneIsNOISZ)
        {
            try
            {
                if (noiszBGObject != null && UNANIMATED.enableSceneSwitching.Value && succeededPreloading)
                {
                    if (sceneIsNOISZ)
                    {
                        noiszBGObject.SetActive(true);

                        foreach (DodgeNote spike in RhythmController.Instance.activeNotes.Where(n => n is DodgeNote).Cast<DodgeNote>())
                        {
                            SpriteRenderer renderer = spike.sprite;
                            renderer.sprite = noiszSpikePrefab.sprite.sprite;
                            renderer.color = noiszSpikePrefab.sprite.color;
                            renderer.transform.localScale = noiszSpikePrefab.sprite.transform.localScale;
                        }

                        RhythmController.Instance.notePrefabGroup.dodgeNotePrefab = noiszSpikePrefab;
                    }
                    else
                    {
                        noiszBGObject.SetActive(false);

                        foreach (DodgeNote spike in RhythmController.Instance.activeNotes.Where(n => n is DodgeNote).Cast<DodgeNote>())
                        {
                            SpriteRenderer renderer = spike.sprite;
                            renderer.sprite = normalSpikePrefab.sprite.sprite;
                            renderer.color = FileStorage.beatmapOptions.dodgeNoteColor;
                            renderer.transform.localScale = normalSpikePrefab.sprite.transform.localScale;
                        }

                        RhythmController.Instance.notePrefabGroup.dodgeNotePrefab = normalSpikePrefab;
                    }
                }
            }
            catch (Exception ex)
            {
                UNANIMATED.Logger.LogWarning($"Failed to perform NOISZ switching logic! {ex.Message}");
                UNANIMATED.Logger.LogWarning(ex.StackTrace);
            }
        }

        private static void DOTransition(float endValue)
        {
            // if (noiszBGImage != null)
            // {
            TweenerCore<Color, Color, ColorOptions> tweenerCore = DOTween.ToAlpha(() => noiszBGImage.color, delegate (Color x) { noiszBGImage.color = x; }, endValue, 0.5f);
            tweenerCore.SetTarget(noiszBGImage);
            // }
        }
    }
}