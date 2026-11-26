using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace UNANIMATED.VideoPlayback
{
    public static class VideoPlaybackController
    {
        public static bool showVideo;
        public static VideoPlayer player;


        public static void Reset()
        {
            showVideo = true;
            player = null;
        }


        public static void FixVideoPlayer(GameObject playerGameObject)
        {
            playerGameObject.GetComponent<RawImage>().material = ShaderMaskingController.bgDimMaterial;

            playerGameObject.transform.localScale = new Vector3(0.671f, 0.671f, 0.671f);
            playerGameObject.transform.localPosition = new Vector3(playerGameObject.transform.localPosition.x, playerGameObject.transform.localPosition.y, 60);
        }


        public static RenderTexture CreateRenderTexture()
        {
            RenderTextureFormat textureFormat = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf) ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGB32;
            RenderTexture renderTexture = new RenderTexture(3840, 2160, 32, textureFormat)
            {
                useMipMap = false,
                autoGenerateMips = false,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                antiAliasing = 1
            };

            return renderTexture;
        }


        public static void ParseCommand(CommandEventInfo currentCommand)
        {
            VideoOption type = currentCommand.GetEnumParamForced<VideoOption>(0);

            UNANIMATED.Logger.LogInfo($"Parsed video command at {currentCommand.Time} ms: {type} | params {currentCommand.ParamString} | length {currentCommand.Duration:0} ms");

            switch (type)
            {
                case VideoOption.Hide:
                    showVideo = false;
                    break;
                case VideoOption.Show:
                    showVideo = true;
                    break;
                case VideoOption.AspectRatioMode:
                    player.aspectRatio = currentCommand.GetEnumParam<VideoAspectRatio>(1);
                    break;
            }
        }
    }
}