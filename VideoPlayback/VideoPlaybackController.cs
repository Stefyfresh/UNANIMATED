using UnityEngine;
using UnityEngine.UI;

namespace UNANIMATED.VideoPlayback
{
    public static class VideoPlaybackController
    {
        public static void FixVideoPlayer(GameObject playerGameObject)
        {
            playerGameObject.GetComponent<RawImage>().material = ShaderMaskingController.bgDimMaterial;

            playerGameObject.transform.localScale = new Vector3(0.671f, 0.671f, 0.671f);
            playerGameObject.transform.localPosition = new Vector3(playerGameObject.transform.localPosition.x, playerGameObject.transform.localPosition.y, 60);
        }


        // RenderTexture currentRT = myRawImage.texture as RenderTexture;

        // if (currentRT != null)
        // {
        //     // Release the old texture from graphics memory
        //     currentRT.Release();

        //     // Set new dimensions
        //     currentRT.width = targetWidth;
        //     currentRT.height = targetHeight;

        //     // Re-create the texture with new settings
        //     currentRT.Create();
        // }
    }
}