using UnityEngine;

namespace UNANIMATED
{
    public static class Utils
    {
        public static void DestroyChildren(Transform transform)
        {
            while (transform.childCount > 0)
            {
                Object.DestroyImmediate(transform.GetChild(0).gameObject);
            }
        }
    }
}