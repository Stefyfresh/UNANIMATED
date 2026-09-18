using System.Linq;
using DG.Tweening;
using Rhythm;
using UnityEngine;

namespace UNANIMATED.CameraControl
{
    public static class CameraController
    {
        public static Vector3 leftCameraPeekOffset = new(-1f, 0, 0.5f);
        public static Vector3 rightCameraPeekOffset = new(1f, 0, 0.5f);
        public static float cameraEaseTime = CameraDefaults.cameraEaseTime;
        public static float? cameraEaseTimeOverride;
        public static Ease cameraEaseMode = Ease.OutQuint;
        public static float cameraFOV = CameraDefaults.cameraFOV;
        public static float cameraFOVTarget = CameraDefaults.cameraFOV;
        public static float cameraFOVTime;
        public static float fovSpeed;
        public static Tweener cameraPosTweener;
        public static bool requestingCameraPosChange;
        public static bool isControllingCamera;
        public static bool legacyCameraUnit;

        public static float CameraEaseTime
        {
            get
            {
                if (cameraEaseTimeOverride != null) return cameraEaseTimeOverride.Value;
                else return cameraEaseTime;
            }
        }

        public static void Reset()
        {
            isControllingCamera = false;
            cameraEaseTime = CameraDefaults.cameraEaseTime;
            cameraFOV = CameraDefaults.cameraFOV;
            cameraFOVTarget = CameraDefaults.cameraFOV;
            cameraEaseMode = CameraDefaults.cameraEaseMode;
            cameraEaseTimeOverride = null;
            requestingCameraPosChange = false;
            legacyCameraUnit = false;
        }


        public static void ParseCommand(CommandEventInfo currentCommand)
        {
            // Get relevant variables
            CameraOverride type = currentCommand.GetEnumParamForced<CameraOverride>(0);
            float secondData = currentCommand.GetFloatParam(1);
            float thirdData = currentCommand.GetFloatParam(2);
            float time = currentCommand.Duration / 1000f;

            // Logging
            UNANIMATED.Logger.LogInfo($"Parsed camera command at {currentCommand.Time} ms: {type} | params {currentCommand.ParamString} | length {time * 1000:0} ms");


            // Command logic
            switch (type)
            {
                case CameraOverride.CameraTarget:
                    if (currentCommand.HasEndTime) cameraEaseTimeOverride = new float?(time);

                    RhythmCameraHelpers.SetCameraPoint(currentCommand.GetEnumParamForced<CameraPoint>(1));
                    break;

                case CameraOverride.CustomCameraTarget:
                    if (currentCommand.HasEndTime) cameraEaseTimeOverride = new float?(time);
                    float fourthData = currentCommand.GetFloatParam(3);

                    RhythmCameraHelpers.SetCustomCameraPoint(secondData, thirdData, fourthData);
                    break;

                case CameraOverride.EaseTime:
                    cameraEaseTime = secondData / 1000f;
                    break;

                case CameraOverride.EaseMode:
                    cameraEaseMode = currentCommand.GetEnumParamForced<Ease>(1);
                    break;

                // TODO: fix these
                case CameraOverride.Shake:
                    if (thirdData != 0) RhythmCameraHelpers.Shake(time, secondData, thirdData);
                    else RhythmCameraHelpers.Shake(time, secondData, secondData);
                    break;

                case CameraOverride.ChromaticAbberation:
                    RhythmCameraHelpers.Shake(time, 0, secondData);
                    break;

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

                    // Kill old tween
                    if (cameraPosTweener != null && cameraPosTweener.IsActive())
                    {
                        cameraPosTweener.Kill();
                    }

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


            if ((double)instance.camera.m_Lens.Aspect < 1.7)
            {
                instance.camera.m_Lens.FieldOfView = Camera.HorizontalToVerticalFieldOfView(Camera.VerticalToHorizontalFieldOfView(cameraFOV, 1.778f), instance.camera.m_Lens.Aspect);
            }
            else
            {
                instance.camera.m_Lens.FieldOfView = cameraFOV;
            }
            instance.prevAspect = instance.camera.m_Lens.Aspect;
            cameraFOV = Mathf.SmoothDamp(cameraFOV, cameraFOVTarget, ref fovSpeed, cameraFOVTime);

            instance.horizontalOffset = Mathf.SmoothDamp(instance.horizontalOffset, instance.horizontalTarget, ref instance.horizontalSpeed, instance.horizontalTime);
            instance.offSetPosition.x = instance.originalPosition.x + instance.horizontalOffset;
            instance.offSetPosition.y = instance.originalPosition.y;
            instance.zoomOffset = Mathf.SmoothDamp(instance.zoomOffset, instance.zoomTarget, ref instance.zoomSpeed, instance.zoomTime);
            instance.offSetPosition.z = instance.originalPosition.z + instance.zoomOffset;
            instance.rotationOffset = Mathf.SmoothDamp(instance.rotationOffset, instance.rotationTarget, ref instance.rotationSpeed, instance.rotationTime);
            instance.offsetRotation.z = instance.originalRotation.z + instance.rotationOffset;
            instance.transform.localPosition = instance.offSetPosition + instance.shakeOffset;
            instance.transform.localEulerAngles = instance.offsetRotation;
            instance.chromaticAbberationIntensity = Mathf.MoveTowards(instance.chromaticAbberationIntensity, 0.15f, Time.deltaTime);
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


        public static class CameraDefaults
        {
            public static readonly Ease cameraEaseMode = Ease.OutQuint;
            public static readonly float cameraEaseTime = 0.7f;
            public static readonly float cameraFOV = 60f;
        }
    }
}