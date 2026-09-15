using UnityEngine;
using UnityEngine.UI;

namespace UNANIMATED
{
    public static class ShaderMaskingController
    {
        public static Material bgDimMaterial;

        public static void Reset()
        {
            // bgDimMaterial = null;
        }

        public static void GetCorrectShaders()
        {
            if (bgDimMaterial == null)
            {
                GameObject bgDimObject = GameObject.Find("/Rhythm Game Container/RhythmUI/UiParentCanvas/UiParent/Masks/BackgroundDim");
                Image bgDimImage = bgDimObject?.GetComponent<Image>();
                bgDimMaterial = bgDimImage?.material;
            }
        }
    }
}