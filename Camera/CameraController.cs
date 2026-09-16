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
        public static Ease cameraEaseMode = Ease.OutQuint;
        public static float cameraFOV = CameraDefaults.cameraFOV;
        public static float cameraFOVTarget = CameraDefaults.cameraFOV;
        public static float cameraFOVTime;
        public static float fovSpeed;
        public static Tweener cameraPosTweener;
        public static bool requestingCameraPosChange;

        public static void Reset()
        {
            UNANIMATED.isControllingCamera = false;
            cameraEaseTime = CameraDefaults.cameraEaseTime;
            cameraFOV = CameraDefaults.cameraFOV;
            cameraFOVTarget = CameraDefaults.cameraFOV;
            cameraEaseMode = CameraDefaults.cameraEaseMode;
            requestingCameraPosChange = false;
        }


        public static void ParseCommand(CommandEventInfo currentCommand)
        {
            // Get relevant variables
            CameraOverride type = currentCommand.GetEnumParam<CameraOverride>(0);
            float secondData = currentCommand.GetFloatParam(1);
            float time = currentCommand.Duration / 1000f;

            // Logging
            UNANIMATED.Logger.LogInfo($"Parsed camera command at {currentCommand.Time} ms: {type} | params {currentCommand.ParamString} | length {time * 1000:0} ms");

            // TODO: Fix shake and add chromatic abberation as a separate setting
            // Command logic
            switch (type)
            {
                case CameraOverride.CameraTarget:
                    RhythmCameraHelpers.SetCameraPoint(currentCommand.GetEnumParam<CameraPoint>(1));
                    break;

                case CameraOverride.CustomCameraTarget:
                    float thirdData = currentCommand.GetFloatParam(2);
                    float fourthData = currentCommand.GetFloatParam(3);
                    UNANIMATED.isControllingCamera = false;
                    RhythmCamera.instance.SetTargetPoint(new Vector3(secondData / 10f, thirdData / 10f, fourthData / 10f));
                    UNANIMATED.isControllingCamera = true;
                    break;

                case CameraOverride.EaseTime:
                    cameraEaseTime = secondData / 1000f;
                    break;

                case CameraOverride.EaseMode:
                    cameraEaseMode = currentCommand.GetEnumParam<Ease>(1);
                    break;

                case CameraOverride.Shake:
                    RhythmCameraHelpers.Shake(time, secondData, 0);
                    break;

                case CameraOverride.ChromaticAbberation:
                    RhythmCameraHelpers.Shake(time, 0, secondData);
                    break;

                default:
                    {
                        float amount = secondData / 10f;
                        // Other mode
                        if (time >= 0)
                        {
                            // Command has a time
                            switch (type)
                            {
                                case CameraOverride.Reset:
                                    bool resetOtherParams = secondData == 0;

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
                                    RhythmCameraHelpers.ZoomOffset(amount, time);
                                    break;

                                case CameraOverride.ZoomTarget:
                                    RhythmCameraHelpers.SetZoomTarget(amount, time);
                                    break;

                                case CameraOverride.RotOffset:
                                    RhythmCameraHelpers.RotOffset(secondData, time);
                                    break;

                                case CameraOverride.RotTarget:
                                    RhythmCameraHelpers.SetRotTarget(secondData, time);
                                    break;

                                case CameraOverride.HorizontalOffset:
                                    RhythmCameraHelpers.HorizontalOffset(amount, time);
                                    break;

                                case CameraOverride.HorizontalTarget:
                                    RhythmCameraHelpers.HorizontalOffset(amount, time);
                                    break;

                                case CameraOverride.FOVTarget:
                                    RhythmCameraHelpers.SetFOVTarget(secondData, time);
                                    break;

                                case CameraOverride.FOVOffset:
                                    RhythmCameraHelpers.SetFOVOffset(secondData, time);
                                    break;
                            }

                        }
                        else
                        {
                            // Command is instant
                            switch (type)
                            {
                                case CameraOverride.Reset:
                                    bool resetOtherParams = secondData == 1;

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
                        cameraEaseTime
                    ).SetEase(cameraEaseMode).SetAutoKill(true);
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