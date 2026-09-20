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
        // Prefabs
        private static DodgeNote normalSpikePrefab;
        private static DodgeNote noiszSpikePrefab;

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
                    // normalSpikeSpriteObject = normalSpikePrefab.transform.GetChild(0).gameObject;
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
                    if (!noiszModeOn && !controller.song.over && controller.activeNotes.Any(n => n is DodgeNote))
                    {
                        DOTransition(0.992f);
                        noiszModeOn = true;
                    }
                    if (controller.song.over && noiszModeOn)
                    {
                        DOTransition(0);
                        noiszModeOn = false;
                    }
                }
            }
            catch (Exception) { }
        }


        public static void SpecialPreloadNOISZStage(GameObject controllerRoot)
        {
            try
            {
                if (UNANIMATED.enableSceneSwitching.Value && succeededPreloading)
                {
                    // noiszSpikeSprite = controllerRoot.GetComponentInChildren<RhythmController>()?.notePrefabGroups[0]?.dodgeNotePrefab?.sprite.sprite;
                    noiszSpikePrefab = controllerRoot.GetComponentInChildren<RhythmController>().notePrefabGroups[0].dodgeNotePrefab;
                    // noiszSpikeSpriteObject = noiszSpikePrefab.transform.GetChild(0).gameObject;

                    // noiszSpikeObject = controllerRoot.GetComponentInChildren<RhythmController>()?.notePrefabGroups[0]?.dodgeNotePrefab.transform.GetChild(0).gameObject;
                    GameObject noiszMask = controllerRoot.GetComponentInChildren<NOISZBGController>().transform.Find("BackgroundFill").gameObject;

                    noiszBGObject = UnityEngine.Object.Instantiate(noiszMask, normalMasksParent);
                    // noiszBGObject = noiszMasks.transform.Find("BackgroundFill").gameObject;
                    // noiszBGObject.transform.parent = normalMasksParent;
                    noiszBGObject.name = "NOISZ_BG";
                    noiszBGObject.transform.localPosition = noiszMask.transform.localPosition;
                    noiszBGObject.transform.localEulerAngles = noiszMask.transform.localEulerAngles;
                    noiszBGObject.transform.localScale = noiszMask.transform.localScale;
                    // noiszBGObject.transform.localPosition = new Vector3(0, -0.2f, 19.7f);

                    noiszBGImage = noiszBGObject.GetComponent<Image>();

                    // if (noiszMasks != null)
                    // {
                    //     // GameObject newNoiszMasks = GameObject.Instantiate(noiszMasks, currentUIParent);
                    //     // noiszMasks.transform.parent = normalUIParent;
                    //     // noiszMasks.GetComponentInChildren<NOISZBGController>().controller = RhythmController.Instance;
                    //     // GameObject.Destroy(noiszMasks.GetComponent<RhythmStencilMasks>());
                    //     // GameObject.Destroy(noiszMasks.GetComponentInChildren<VideoPlayer>());
                    //     noiszBGObject = noiszMasks.transform.Find("BackgroundFill").gameObject;
                    // }

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

                        for (int i = 0; i < RhythmController.Instance.activeNotes.Count(); i++)
                        {
                            BaseNote note = RhythmController.Instance.activeNotes[i];
                            // if (note is DodgeNote spike)
                            // {
                            //     if (spike.transform.childCount == 1) UnityEngine.Object.Instantiate(noiszSpikeSpriteObject, spike.transform).name = "NOISZ";
                            //     spike.transform.GetChild(1).gameObject.SetActive(true);
                            //     spike.sprite.gameObject.SetActive(false);
                            // }
                            foreach (DodgeNote spike in RhythmController.Instance.activeNotes.Where(n => n is DodgeNote))
                            {
                                SpriteRenderer renderer = spike.sprite;
                                renderer.sprite = noiszSpikePrefab.sprite.sprite;
                                renderer.color = noiszSpikePrefab.sprite.color;
                                renderer.transform.localScale = noiszSpikePrefab.sprite.transform.localScale;
                                // if (spike.transform.childCount == 1) UnityEngine.Object.Instantiate(noiszSpikeSpriteObject, spike.transform);
                                // spike.transform.GetChild(0).gameObject.SetActive(false);
                                // spike.transform.GetChild(1).gameObject.SetActive(true);
                                // spike.sprite.gameObject.SetActive(false);
                            }

                        }
                        RhythmController.Instance.notePrefabGroup.dodgeNotePrefab = noiszSpikePrefab;
                    }
                    else
                    {
                        noiszBGObject.SetActive(false);

                        // for (int i = 0; i < RhythmController.Instance.activeNotes.Count(); i++)
                        // {
                        //     BaseNote note = RhythmController.Instance.activeNotes[i];
                        //     if (note is DodgeNote spike)
                        //     {
                        //         spike.transform.GetChild(1).gameObject.SetActive(false);
                        //         spike.sprite.gameObject.SetActive(true);
                        //     }
                        // }
                        foreach (DodgeNote spike in RhythmController.Instance.activeNotes.Where(n => n is DodgeNote))
                        {
                            SpriteRenderer renderer = spike.sprite;
                            renderer.sprite = normalSpikePrefab.sprite.sprite;
                            renderer.color = FileStorage.beatmapOptions.dodgeNoteColor;
                            renderer.transform.localScale = normalSpikePrefab.sprite.transform.localScale;
                            // if (spike.transform.childCount == 1) UnityEngine.Object.Instantiate(normalSpikeSpriteObject, spike.transform).transform.SetAsFirstSibling();
                            // spike.transform.GetChild(1).gameObject.SetActive(false);
                            // spike.sprite.gameObject.SetActive(true);
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
            if (noiszBGImage != null)
            {
                TweenerCore<Color, Color, ColorOptions> tweenerCore = DOTween.ToAlpha(() => noiszBGImage.color, delegate (Color x) { noiszBGImage.color = x; }, endValue, 0.5f);
                tweenerCore.SetTarget(noiszBGImage);
            }
        }
    }
}