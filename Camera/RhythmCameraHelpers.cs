using DG.Tweening;
using Rhythm;
using UnityEngine;

namespace UNANIMATED.CameraControl
{
    public static class RhythmCameraHelpers
    {
        /// <summary>
        /// Shakes the camera for a certain time and by a certain amount.
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="amount"></param>
        /// <param name="chromaticAbberationAmount"></param>
        public static void Shake(float duration, float amount, float chromaticAbberationAmount)
        {
            RhythmCamera.instance.StopAllCoroutines();
            CameraOperator.VhsVerticalValueTweener.SetValue(0.5f * chromaticAbberationAmount);
            RhythmCamera.instance.StartCoroutine(RhythmCamera.instance.ShakeCoroutine(duration, amount, chromaticAbberationAmount));
        }


        /// <summary>
        /// Offsets the current horizontal position of the camera by a certain amount.
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="time"></param>
        /// <param name="reverseFromDestination"></param>
        public static void HorizontalOffset(float amount, float time, bool reverseFromDestination = false)
        {
            if (!reverseFromDestination)
            {
                RhythmCamera.instance.horizontalTarget += amount;
            }
            else
            {
                RhythmCamera.instance.horizontalOffset += amount;
            }
            RhythmCamera.instance.horizontalTime = time;
        }


        /// <summary>
        /// Offsets the current zoom of the camera by a certain amount after a certain amount of time.
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="time"></param>
        /// <param name="reverseFromDestination"></param>
        public static void ZoomOffset(float amount, float time, bool reverseFromDestination = false)
        {
            if (!reverseFromDestination)
            {
                RhythmCamera.instance.zoomTarget += amount;
            }
            else
            {
                RhythmCamera.instance.zoomOffset += amount;
            }
            RhythmCamera.instance.zoomTime = time;
        }


        /// <summary>
        /// Offsets the current rotation of the camera by a certain amount after a certain amount of time.
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="time"></param>
        /// <param name="reverseFromDestination"></param>
        public static void RotOffset(float amount, float time, bool reverseFromDestination = false)
        {
            if (!reverseFromDestination)
            {
                RhythmCamera.instance.rotationTarget += amount;
            }
            else
            {
                RhythmCamera.instance.rotationOffset += amount;
            }
            RhythmCamera.instance.rotationTime = time;
        }


        /// <summary>
        /// Immediately sets the zoom of the camera.
        /// </summary>
        /// <param name="amount"></param>
        public static void SetZoomOffsetImmediate(float amount)
        {
            RhythmCamera.instance.zoomTarget += amount;
            RhythmCamera.instance.zoomOffset += amount;
        }


        /// <summary>
        /// Immediately sets the zoom of the camera.
        /// </summary>
        /// <param name="amount"></param>
        public static void SetZoomTargetImmediate(float amount)
        {
            RhythmCamera.instance.zoomTarget = amount;
            RhythmCamera.instance.zoomOffset = amount;
        }


        /// <summary>
        /// Sets the target zoom and zooms to that position after a certain amount of time.
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="time"></param>
        public static void SetZoomTarget(float amount, float time)
        {
            RhythmCamera.instance.zoomTarget = amount;
            RhythmCamera.instance.zoomTime = time;
        }


        /// <summary>
        /// Immediately sets the rotation of the camera.
        /// </summary>
        /// <param name="amount"></param>
        public static void SetRotTargetImmediate(float amount)
        {
            RhythmCamera.instance.rotationOffset = amount;
            RhythmCamera.instance.rotationTarget = amount;
        }


        /// <summary>
        /// Immediately sets the rotation of the camera.
        /// </summary>
        /// <param name="amount"></param>
        public static void SetRotOffsetImmediate(float amount)
        {
            RhythmCamera.instance.rotationOffset += amount;
            RhythmCamera.instance.rotationTarget += amount;
        }


        /// <summary>
        /// Sets the target rotation and rotates to that position after a certain amount of time.
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="time"></param>
        public static void SetRotTarget(float amount, float time)
        {
            RhythmCamera.instance.rotationTarget = amount;
            RhythmCamera.instance.rotationTime = time;
        }


        /// <summary>
        /// Immediately sets the horizontal position of the camera.
        /// </summary>
        /// <param name="amount"></param>
        public static void SetHorizontalTargetImmediate(float amount)
        {
            RhythmCamera.instance.horizontalOffset = amount;
            RhythmCamera.instance.horizontalTarget = amount;
        }


        /// <summary>
        /// Immediately sets the horizontal offset of the camera.
        /// </summary>
        /// <param name="amount"></param>
        public static void SetHorizontalOffsetImmediate(float amount)
        {
            RhythmCamera.instance.horizontalOffset += amount;
            RhythmCamera.instance.horizontalTarget += amount;
        }


        /// <summary>
        /// Sets the target horizontal position and moves to that position after a certain amount of time.
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="time"></param>
        public static void SetHorizontalTarget(float amount, float time)
        {
            RhythmCamera.instance.horizontalTarget = amount;
            RhythmCamera.instance.horizontalTime = time;
        }

        public static void SetFOVTarget(float amount, float time)
        {
            CameraController.cameraFOVTime = time;
            CameraController.cameraFOVTarget = amount;
        }

        /// <summary>
        /// Offsets the current FOV of the camera by a certain amount after a certain amount of time.
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="time"></param>
        /// <param name="reverseFromDestination"></param>
        public static void FOVOffset(float amount, float time, bool reverseFromDestination = false)
        {
            if (!reverseFromDestination)
            {
                CameraController.cameraFOVTarget += amount;
            }
            else
            {
                CameraController.cameraFOV += amount;
            }
            CameraController.cameraFOVTime = time;
        }

        public static void SetFOVTargetImmediate(float amount)
        {
            CameraController.cameraFOV = amount;
            CameraController.cameraFOVTarget = amount;
        }

        public static void SetFOVOffsetImmediate(float amount)
        {
            CameraController.cameraFOV += amount;
            CameraController.cameraFOVTarget += amount;
        }


        /// <summary>
        /// Sets the camera point out of one of the 5 default base game camera points.
        /// </summary>
        /// <param name="point"></param>
        public static void SetCameraPoint(CameraPoint point)
        {
            CameraController.isControllingCamera = false;
            switch (point)
            {
                case CameraPoint.Left:
                    RhythmCamera.instance.SetTargetPoint(RhythmController.Instance.leftCameraTargetPoint);
                    break;
                case CameraPoint.LeftWide:
                    RhythmCamera.instance.SetTargetPoint(RhythmController.Instance.centerCameraTargetPoint + CameraController.leftCameraPeekOffset);
                    break;
                case CameraPoint.Wide:
                    RhythmCamera.instance.SetTargetPoint(RhythmController.Instance.centerCameraTargetPoint);
                    break;
                case CameraPoint.RightWide:
                    RhythmCamera.instance.SetTargetPoint(RhythmController.Instance.centerCameraTargetPoint + CameraController.rightCameraPeekOffset);
                    break;
                case CameraPoint.Right:
                    RhythmCamera.instance.SetTargetPoint(RhythmController.Instance.rightCameraTargetPoint);
                    break;
            }

            // Immediately apply if ease time is instant
            if (CameraController.CameraEaseTime == 0) RhythmCamera.instance.originalPosition = RhythmCamera.instance.cameraPositionTarget;

            CameraController.isControllingCamera = true;
        }



        /// <summary>
        /// Sets a custom camera point at any X, Y, Z value.
        /// </summary>
        /// <param name="point"></param>
        public static void SetCustomCameraPoint(float x, float y, float z)
        {
            CameraController.isControllingCamera = false;

            if (CameraController.legacyCameraUnit) RhythmCamera.instance.SetTargetPoint(new Vector3(x / 10f, y / 10f, z / 10f));
            else RhythmCamera.instance.SetTargetPoint(new Vector3(x, y, z));

            // Immediately apply if ease time is instant
            if (CameraController.CameraEaseTime == 0) RhythmCamera.instance.originalPosition = RhythmCamera.instance.cameraPositionTarget;

            CameraController.isControllingCamera = true;
        }


        // /// <summary>
        // /// Immediately sets the camera point.
        // /// </summary>
        // /// <param name="point"></param>
        // public static void SetCameraPointImmediate(CameraPoint point)
        // {
        //     SetCameraPoint(point);
        //     Vector3 updatedTarget = RhythmCamera.instance.cameraPositionTarget;
        //     RhythmCamera.instance.transform.localPosition = updatedTarget;
        //     RhythmCamera.instance.originalPosition = updatedTarget;
        // }


        /// <summary>
        /// Reset the camera position back to where it is supposed to be.
        /// </summary>
        public static void ResetCameraPos()
        {
            CameraController.isControllingCamera = false;

            RhythmController controller = RhythmController.Instance;
            if (controller.cameraIsCentered)
            {
                controller.cameraObject.SetTargetPoint(controller.centerCameraTargetPoint);
                controller.onCenterCamera.Invoke();
            }
            else if (controller.indicatingFlip)
            {
                if (controller.player.side == Side.Right)
                {
                    if (FileStorage.options.cameraSwapMode == CameraSwapMode.ZoomOut || FileStorage.options.cameraSwapMode == CameraSwapMode.Automatic)
                    {
                        controller.cameraObject.SetTargetPoint(controller.centerCameraTargetPoint + CameraController.leftCameraPeekOffset);
                        controller.onLeftCameraUpcoming.Invoke();
                    }
                }
                else
                {
                    if (FileStorage.options.cameraSwapMode == CameraSwapMode.ZoomOut || FileStorage.options.cameraSwapMode == CameraSwapMode.Automatic)
                    {
                        controller.cameraObject.SetTargetPoint(controller.centerCameraTargetPoint + CameraController.rightCameraPeekOffset);
                        controller.onRightCameraUpcoming.Invoke();
                    }
                }
            }
            else if (controller.player.side == Side.Right)
            {
                controller.cameraObject.SetTargetPoint(controller.rightCameraTargetPoint);
                controller.onRightCamera.Invoke();
            }
            else if (controller.player.side == Side.Left)
            {
                controller.cameraObject.SetTargetPoint(controller.leftCameraTargetPoint);
                controller.onLeftCamera.Invoke();
            }
        }
    }
}