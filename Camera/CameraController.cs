using System.Linq;
using DG.Tweening;
using Rhythm;
using UnityEngine;

namespace UNANIMATED.CameraControl
{
    public static class CameraController
    {
        // Constants
        public static readonly Vector3 leftCameraPeekOffset = new(-1f, 0, 0.5f);
        public static readonly Vector3 rightCameraPeekOffset = new(1f, 0, 0.5f);

        // Values
        public static float cameraEaseTime = CameraDefaults.cameraEaseTime;
        public static float? cameraEaseTimeOverride;
        public static float? cameraRotEaseTimeOverride;
        public static Ease cameraEaseMode = Ease.OutQuint;
        public static float cameraFOV = CameraDefaults.cameraFOV;
        public static float cameraFOVTarget = CameraDefaults.cameraFOV;
        public static float cameraFOVTime;
        public static float fovSpeed;
        public static Tweener cameraPosTweener;
        public static Tweener cameraRotTweener;

        // States
        public static bool requestingCameraPosChange;
        public static bool requestingCameraRotChange;
        public static bool isControllingCamera;
        public static bool legacyCameraUnit;


        public static float CameraEaseTime { get { return cameraEaseTimeOverride != null ? cameraEaseTimeOverride.Value : cameraEaseTime; } }

        public static float CameraRotEaseTime { get { return cameraRotEaseTimeOverride != null ? cameraRotEaseTimeOverride.Value : cameraEaseTime; } }

        public static void Reset()
        {
            isControllingCamera = false;
            cameraEaseTime = CameraDefaults.cameraEaseTime;
            cameraFOV = CameraDefaults.cameraFOV;
            cameraFOVTarget = CameraDefaults.cameraFOV;
            cameraEaseMode = CameraDefaults.cameraEaseMode;
            cameraEaseTimeOverride = null;
            requestingCameraPosChange = false;
            requestingCameraRotChange = false;
            legacyCameraUnit = false;
            KillTween();
            KillRotTween();
        }


        public static void ParseCommand(CommandEventInfo currentCommand)
        {
            // Get relevant variables
            float time = currentCommand.Duration / 1000f;
            CameraOverride type = currentCommand.GetEnumParamForced<CameraOverride>(0);
            float secondData = currentCommand.GetFloatParam(1);
            float thirdData = currentCommand.GetFloatParam(2);
            float fourthData = currentCommand.GetFloatParam(3);
            CameraAdditionalOptions additionalOptions = currentCommand.GetEnumParam<CameraAdditionalOptions>(4);

            // Logging
            UNANIMATED.Logger.LogInfo($"Parsed camera command at {currentCommand.Time} ms: {type} | params {currentCommand.ParamString} | length {time * 1000:0} ms");


            // Command logic
            // if (additionalOptions != CameraAdditionalOptions.Normal)


            switch (type)
            {
                case CameraOverride.CameraTarget:
                    if (currentCommand.HasEndTime) cameraEaseTimeOverride = time;
                    else cameraEaseTimeOverride = null;

                    RhythmCameraHelpers.SetCameraPoint(currentCommand.GetEnumParamForced<CameraPoint>(1));
                    break;

                case CameraOverride.CustomCameraTarget:
                    if (additionalOptions == CameraAdditionalOptions.StartPoint)
                    {
                        cameraEaseTimeOverride = 0;
                        RhythmCameraHelpers.SetCustomCameraPoint(currentCommand.GetFloatParam(5), currentCommand.GetFloatParam(6), currentCommand.GetFloatParam(7));
                    }
                    if (currentCommand.HasEndTime || additionalOptions == CameraAdditionalOptions.Instant) cameraEaseTimeOverride = time;
                    else cameraEaseTimeOverride = null;


                    RhythmCameraHelpers.SetCustomCameraPoint(secondData, thirdData, fourthData);
                    break;

                case CameraOverride.CustomRotTarget:
                    if (additionalOptions == CameraAdditionalOptions.StartPoint)
                    {
                        cameraRotEaseTimeOverride = 0;
                        RhythmCameraHelpers.SetCustomCameraRot(currentCommand.GetFloatParam(5), currentCommand.GetFloatParam(6), currentCommand.GetFloatParam(7));
                    }
                    if (currentCommand.HasEndTime || additionalOptions == CameraAdditionalOptions.Instant) cameraRotEaseTimeOverride = time;
                    else cameraRotEaseTimeOverride = null;

                    RhythmCameraHelpers.SetCustomCameraRot(secondData, thirdData, fourthData);
                    break;

                case CameraOverride.CustomCameraOffset:
                    if (additionalOptions == CameraAdditionalOptions.StartPoint)
                    {
                        cameraEaseTimeOverride = 0;
                        RhythmCameraHelpers.SetCustomCameraPointOffset(currentCommand.GetFloatParam(5), currentCommand.GetFloatParam(6), currentCommand.GetFloatParam(7));
                    }
                    if (currentCommand.HasEndTime || additionalOptions == CameraAdditionalOptions.Instant) cameraEaseTimeOverride = time;
                    else cameraEaseTimeOverride = null;

                    RhythmCameraHelpers.SetCustomCameraPointOffset(secondData, thirdData, fourthData, additionalOptions == CameraAdditionalOptions.Reverse);
                    break;

                case CameraOverride.CustomRotOffset:
                    if (additionalOptions == CameraAdditionalOptions.StartPoint)
                    {
                        cameraRotEaseTimeOverride = 0;
                        RhythmCameraHelpers.SetCustomCameraRotOffset(currentCommand.GetFloatParam(5), currentCommand.GetFloatParam(6), currentCommand.GetFloatParam(7));
                    }
                    if (currentCommand.HasEndTime || additionalOptions == CameraAdditionalOptions.Instant) cameraRotEaseTimeOverride = time;
                    else cameraRotEaseTimeOverride = null;

                    RhythmCameraHelpers.SetCustomCameraRotOffset(secondData, thirdData, fourthData, additionalOptions == CameraAdditionalOptions.Reverse);
                    break;

                case CameraOverride.EaseTime:
                    cameraEaseTime = secondData / 1000f;
                    break;

                case CameraOverride.EaseMode:
                    cameraEaseMode = currentCommand.GetEnumParamForced<Ease>(1);
                    break;

                // TODO: fix these
                // case CameraOverride.Shake:
                //     if (thirdData != 0) RhythmCameraHelpers.Shake(time, secondData, thirdData);
                //     else RhythmCameraHelpers.Shake(time, secondData, secondData);
                //     break;

                // case CameraOverride.ChromaticAbberation:
                //     RhythmCameraHelpers.Shake(time, 0, secondData);
                //     break;

                default:
                    {
                        float amount = secondData;
                        if (legacyCameraUnit) amount = secondData / 10f;
                        bool reverse = currentCommand.GetBoolParam(2);
                        // Other mode
                        if (time >= 0)
                        {
                            // Command has a time
                            switch (type)
                            {
                                case CameraOverride.Reset:
                                    bool resetOtherParams = !currentCommand.GetBoolParam(1);

                                    RhythmCameraHelpers.ResetCameraPos();
                                    if (resetOtherParams)
                                    {
                                        RhythmCameraHelpers.SetRotTarget(0, time);
                                        RhythmCameraHelpers.SetZoomTarget(0, time);
                                        RhythmCameraHelpers.SetHorizontalTarget(0, time);
                                        RhythmCameraHelpers.SetFOVTarget(CameraDefaults.cameraFOV, time);
                                        cameraEaseTime = CameraDefaults.cameraEaseTime;
                                        cameraEaseMode = CameraDefaults.cameraEaseMode;
                                    }
                                    break;

                                case CameraOverride.ZoomOffset:
                                    RhythmCameraHelpers.ZoomOffset(amount, time, reverse);
                                    break;

                                case CameraOverride.ZoomTarget:
                                    RhythmCameraHelpers.SetZoomTarget(amount, time);
                                    break;

                                case CameraOverride.RotOffset:
                                    RhythmCameraHelpers.RotOffset(secondData, time, reverse);
                                    break;

                                case CameraOverride.RotTarget:
                                    RhythmCameraHelpers.SetRotTarget(secondData, time);
                                    break;

                                case CameraOverride.HorizontalOffset:
                                    RhythmCameraHelpers.HorizontalOffset(amount, time, reverse);
                                    break;

                                case CameraOverride.HorizontalTarget:
                                    RhythmCameraHelpers.SetHorizontalTarget(amount, time);
                                    break;

                                case CameraOverride.FOVTarget:
                                    RhythmCameraHelpers.SetFOVTarget(secondData, time);
                                    break;

                                case CameraOverride.FOVOffset:
                                    RhythmCameraHelpers.FOVOffset(secondData, time, reverse);
                                    break;
                            }

                        }
                        else
                        {
                            // Command is instant
                            switch (type)
                            {
                                case CameraOverride.Reset:
                                    bool resetOtherParams = !currentCommand.GetBoolParam(1);

                                    RhythmCameraHelpers.ResetCameraPos();
                                    if (resetOtherParams)
                                    {
                                        RhythmCameraHelpers.SetRotTargetImmediate(0);
                                        RhythmCameraHelpers.SetZoomTargetImmediate(0);
                                        RhythmCameraHelpers.SetHorizontalTargetImmediate(0);
                                        RhythmCameraHelpers.SetFOVTargetImmediate(CameraDefaults.cameraFOV);
                                        cameraEaseTime = CameraDefaults.cameraEaseTime;
                                        cameraEaseMode = CameraDefaults.cameraEaseMode;
                                    }
                                    break;

                                case CameraOverride.ZoomOffset:
                                    RhythmCameraHelpers.SetZoomOffsetImmediate(amount);
                                    break;

                                case CameraOverride.ZoomTarget:
                                    RhythmCameraHelpers.SetZoomTargetImmediate(amount);
                                    break;

                                case CameraOverride.RotOffset:
                                    RhythmCameraHelpers.SetRotOffsetImmediate(secondData);
                                    break;

                                case CameraOverride.RotTarget:
                                    RhythmCameraHelpers.SetRotTargetImmediate(secondData);
                                    break;

                                case CameraOverride.HorizontalOffset:
                                    RhythmCameraHelpers.SetHorizontalOffsetImmediate(amount);
                                    break;

                                case CameraOverride.HorizontalTarget:
                                    RhythmCameraHelpers.SetHorizontalTargetImmediate(amount);
                                    break;

                                case CameraOverride.FOVTarget:
                                    RhythmCameraHelpers.SetFOVTargetImmediate(secondData);
                                    break;

                                case CameraOverride.FOVOffset:
                                    RhythmCameraHelpers.SetFOVOffsetImmediate(secondData);
                                    break;
                            }
                        }
                    }
                    break;
            }
        }



        public static void DoFixedUpdate(RhythmCamera instance)
        {
            RhythmCameraUpdatePositionTarget(instance);
            if (UNANIMATED.effectsEnabled)
            {
                if (requestingCameraPosChange)
                {
                    requestingCameraPosChange = false;
                    KillTween();

                    // Custom easing and time
                    cameraPosTweener = DOTween.To(() => instance.originalPosition,
                        delegate (Vector3 x)
                        {
                            instance.originalPosition = x;
                            // UNANIMATED.Logger.LogInfo($"X: {x} | new position: {instance.originalPosition}");
                        },
                        instance.cameraPositionTarget,
                        CameraEaseTime
                    ).SetEase(cameraEaseMode).SetAutoKill(true);
                    cameraEaseTimeOverride = null;
                }

                if (requestingCameraRotChange)
                {
                    requestingCameraRotChange = false;
                    KillRotTween();

                    // Custom easing and time
                    cameraRotTweener = DOTween.To(() => instance.offsetRotation,
                        delegate (Vector3 x)
                        {
                            instance.offsetRotation = x;
                        },
                        RhythmCameraHelpers.rotationTarget,
                        CameraRotEaseTime
                    ).SetEase(cameraEaseMode).SetAutoKill(true);
                    cameraRotEaseTimeOverride = null;
                }
            }
            else
            {
                // Normal easing and time (technically not correct)
                DOTween.To(() => instance.originalPosition,
                    delegate (Vector3 x) { instance.originalPosition = x; },
                    instance.cameraPositionTarget,
                    CameraDefaults.cameraEaseTime
                );
            }

            // FOV
            if (instance.camera.m_Lens.Aspect < 1.7)
            {
                instance.camera.m_Lens.FieldOfView = UnityEngine.Camera.HorizontalToVerticalFieldOfView(UnityEngine.Camera.VerticalToHorizontalFieldOfView(cameraFOV, 1.778f), instance.camera.m_Lens.Aspect);
            }
            else
            {
                instance.camera.m_Lens.FieldOfView = cameraFOV;
            }
            instance.prevAspect = instance.camera.m_Lens.Aspect;
            cameraFOV = Mathf.SmoothDamp(cameraFOV, cameraFOVTarget, ref fovSpeed, cameraFOVTime);


            // Pos
            instance.horizontalOffset = Mathf.SmoothDamp(instance.horizontalOffset, instance.horizontalTarget, ref instance.horizontalSpeed, instance.horizontalTime);
            instance.offSetPosition.x = instance.originalPosition.x + instance.horizontalOffset;
            instance.offSetPosition.y = instance.originalPosition.y;
            instance.zoomOffset = Mathf.SmoothDamp(instance.zoomOffset, instance.zoomTarget, ref instance.zoomSpeed, instance.zoomTime);
            instance.offSetPosition.z = instance.originalPosition.z + instance.zoomOffset;

            // Rot
            instance.rotationOffset = Mathf.SmoothDamp(instance.rotationOffset, instance.rotationTarget, ref instance.rotationSpeed, instance.rotationTime);

            // Apply
            instance.transform.localPosition = instance.offSetPosition + instance.shakeOffset;
            instance.transform.localEulerAngles = instance.originalRotation + instance.offsetRotation + instance.rotationOffset * Vector3.forwardVector; // forward is z = 1
            instance.chromaticAbberationIntensity = Mathf.MoveTowards(instance.chromaticAbberationIntensity, 0.15f, Time.deltaTime);

            // Fix reticle
            Transform child;
            if (instance.transform.childCount > 0 && (child = instance.transform.GetChild(0)).childCount > 1)
            {
                float multiplier = Mathf.Tan(cameraFOV / 2 * Mathf.Deg2Rad) / 0.57735f;
                child.GetChild(1).localScale = new Vector3(multiplier, multiplier, 1) * CameraDefaults.cameraReticleScale;
            }
        }

        public static void RhythmCameraUpdatePositionTarget(RhythmCamera instance)
        {
            // Vector3 prevTarget = instance.cameraPositionTarget;
            instance.cameraPositionTarget = instance.cameraPositionTargetPrimary;
            if (instance.cameraPositionTargetOverride.HasValue)
            {
                instance.cameraPositionTarget = instance.cameraPositionTargetOverride.Value;
            }
            // UNANIMATED.Logger.LogInfo($"Prev target: {prevTarget} | new target: {instance.cameraPositionTarget}");
        }

        public static void KillTween()
        {
            if (cameraPosTweener != null && cameraPosTweener.IsActive()) cameraPosTweener.Kill();
        }

        public static void KillRotTween()
        {
            if (cameraRotTweener != null && cameraRotTweener.IsActive()) cameraRotTweener.Kill();
        }


        public static class CameraDefaults
        {
            public static readonly Ease cameraEaseMode = Ease.OutQuint;
            public static readonly float cameraEaseTime = 0.7f;
            public static readonly float cameraFOV = 60f;
            public static readonly float cameraReticleScale = 0.02288267f;
        }
    }
}