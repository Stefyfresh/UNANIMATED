using DG.Tweening;
using Rhythm;
using UnityEngine;

namespace UNANIMATED.CameraControl
{
    public static class RhythmCameraHelpers
    {
        public static Vector3 rotationTarget;

        private static RhythmCamera Camera { get { return RhythmCamera.instance; } }

        /// <summary>
        /// Shakes the camera for a certain time and by a certain amount.
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="amount"></param>
        /// <param name="chromaticAbberationAmount"></param>
        public static void Shake(float duration, float amount, float chromaticAbberationAmount)
        {
            Camera.StopAllCoroutines();
            CameraOperator.VhsVerticalValueTweener.SetValue(0.5f * chromaticAbberationAmount);
            Camera.StartCoroutine(Camera.ShakeCoroutine(duration, amount, chromaticAbberationAmount));
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
                Camera.horizontalTarget += amount;
            }
            else
            {
                Camera.horizontalOffset += amount;
            }
            Camera.horizontalTime = time;
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
                Camera.zoomTarget += amount;
            }
            else
            {
                Camera.zoomOffset += amount;
            }
            Camera.zoomTime = time;
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
                Camera.rotationTarget += amount;
            }
            else
            {
                Camera.rotationOffset += amount;
            }
            Camera.rotationTime = time;
        }


        /// <summary>
        /// Immediately sets the zoom of the camera.
        /// </summary>
        /// <param name="amount"></param>
        public static void SetZoomOffsetImmediate(float amount)
        {
            Camera.zoomTarget += amount;
            Camera.zoomOffset += amount;
        }


        /// <summary>
        /// Immediately sets the zoom of the camera.
        /// </summary>
        /// <param name="amount"></param>
        public static void SetZoomTargetImmediate(float amount)
        {
            Camera.zoomTarget = amount;
            Camera.zoomOffset = amount;
        }


        /// <summary>
        /// Sets the target zoom and zooms to that position after a certain amount of time.
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="time"></param>
        public static void SetZoomTarget(float amount, float time)
        {
            Camera.zoomTarget = amount;
            Camera.zoomTime = time;
        }


        /// <summary>
        /// Immediately sets the rotation of the camera.
        /// </summary>
        /// <param name="amount"></param>
        public static void SetRotTargetImmediate(float amount)
        {
            Camera.rotationOffset = amount;
            Camera.rotationTarget = amount;
        }


        /// <summary>
        /// Immediately sets the rotation of the camera.
        /// </summary>
        /// <param name="amount"></param>
        public static void SetRotOffsetImmediate(float amount)
        {
            Camera.rotationOffset += amount;
            Camera.rotationTarget += amount;
        }


        /// <summary>
        /// Sets the target rotation and rotates to that position after a certain amount of time.
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="time"></param>
        public static void SetRotTarget(float amount, float time)
        {
            Camera.rotationTarget = amount;
            Camera.rotationTime = time;
        }


        /// <summary>
        /// Immediately sets the horizontal position of the camera.
        /// </summary>
        /// <param name="amount"></param>
        public static void SetHorizontalTargetImmediate(float amount)
        {
            Camera.horizontalOffset = amount;
            Camera.horizontalTarget = amount;
        }


        /// <summary>
        /// Immediately sets the horizontal offset of the camera.
        /// </summary>
        /// <param name="amount"></param>
        public static void SetHorizontalOffsetImmediate(float amount)
        {
            Camera.horizontalOffset += amount;
            Camera.horizontalTarget += amount;
        }


        /// <summary>
        /// Sets the target horizontal position and moves to that position after a certain amount of time.
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="time"></param>
        public static void SetHorizontalTarget(float amount, float time)
        {
            Camera.horizontalTarget = amount;
            Camera.horizontalTime = time;
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
                    Camera.SetTargetPoint(RhythmController.Instance.leftCameraTargetPoint);
                    break;
                case CameraPoint.LeftWide:
                    Camera.SetTargetPoint(RhythmController.Instance.centerCameraTargetPoint + CameraController.leftCameraPeekOffset);
                    break;
                case CameraPoint.Wide:
                    Camera.SetTargetPoint(RhythmController.Instance.centerCameraTargetPoint);
                    break;
                case CameraPoint.RightWide:
                    Camera.SetTargetPoint(RhythmController.Instance.centerCameraTargetPoint + CameraController.rightCameraPeekOffset);
                    break;
                case CameraPoint.Right:
                    Camera.SetTargetPoint(RhythmController.Instance.rightCameraTargetPoint);
                    break;
                case CameraPoint.Reset:
                    ResetCameraPos();
                    return;
            }

            CameraController.KillTween();

            // Immediately apply if ease time is instant
            if (CameraController.CameraEaseTime == 0)
            {
                Camera.originalPosition = Camera.cameraPositionTarget;
            }

            CameraController.isControllingCamera = true;
        }



        /// <summary>
        /// Sets a custom camera point at any X, Y, Z value.
        /// </summary>
        /// <param name="point"></param>
        public static void SetCustomCameraPoint(float x, float y, float z)
        {
            CameraController.isControllingCamera = false;

            if (CameraController.legacyCameraUnit) Camera.SetTargetPoint(new Vector3(x / 10f, y / 10f, z / 10f));
            else Camera.SetTargetPoint(new Vector3(x, y, z));

            CameraController.KillTween();

            // Immediately apply if ease time is instant
            if (CameraController.CameraEaseTime == 0)
            {
                Camera.originalPosition = Camera.cameraPositionTarget;
            }

            CameraController.isControllingCamera = true;
        }




        /// <summary>
        /// Sets a custom camera point at any X, Y, Z value.
        /// </summary>
        /// <param name="point"></param>
        public static void SetCustomCameraPointOffset(float x, float y, float z, bool reverse = false)
        {
            CameraController.isControllingCamera = false;

            if (!reverse) Camera.SetTargetPoint(new Vector3(Camera.cameraPositionTarget.x + x, Camera.cameraPositionTarget.y + y, Camera.cameraPositionTarget.z + z));
            else
            {
                CameraController.requestingCameraPosChange = true;
                Camera.originalPosition = new Vector3(Camera.originalPosition.x + x, Camera.originalPosition.y + y, Camera.originalPosition.z + z);
            }

            CameraController.KillTween();

            // Immediately apply if ease time is instant
            if (CameraController.CameraEaseTime == 0)
            {
                Camera.originalPosition = Camera.cameraPositionTarget;
            }

            CameraController.isControllingCamera = true;
        }



        /// <summary>
        /// Sets a custom camera rotation for any X, Y, Z value.
        /// </summary>
        /// <param name="point"></param>
        public static void SetCustomCameraRot(float x, float y, float z)
        {
            CameraController.requestingCameraRotChange = true;
            CameraController.KillRotTween();

            rotationTarget = new Vector3(x, y, z);

            // Immediately apply if ease time is instant
            if (CameraController.CameraRotEaseTime == 0)
            {
                Camera.offsetRotation = rotationTarget;
            }
        }



        /// <summary>
        /// Sets a custom camera rotation for any X, Y, Z value.
        /// </summary>
        /// <param name="point"></param>
        public static void SetCustomCameraRotOffset(float x, float y, float z, bool reverse = false)
        {
            CameraController.requestingCameraRotChange = true;
            CameraController.KillRotTween();

            if (!reverse) rotationTarget = new Vector3(rotationTarget.x + x, rotationTarget.y + y, rotationTarget.z + z);
            else Camera.offsetRotation = new Vector3(Camera.offsetRotation.x + x, Camera.offsetRotation.y + y, Camera.offsetRotation.z + z);

            // Immediately apply if ease time is instant
            if (CameraController.CameraRotEaseTime == 0)
            {
                Camera.offsetRotation = rotationTarget;
            }
        }




        /// <summary>
        /// Reset the camera position back to where it is supposed to be.
        /// This is a duplicate of the code in RhythmController but that can be called immediately.
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