using System;
using UnityEngine;

namespace Unity.Muse.Texture
{
    internal static class PrimitiveObjectsProvider
    {
        public static GameObject GetPrimitiveInstance(PrimitiveObjectTypes primitiveType)
        {
            var path = string.Empty;
            switch (primitiveType)
            {
                case PrimitiveObjectTypes.Sphere:
                    path = "PreviewSphere";
                    break; 
                case PrimitiveObjectTypes.Cube:
                    path = "PreviewCube";
                    break;
                case PrimitiveObjectTypes.Plane:
                    path = "PreviewPlane";
                    break;
                case PrimitiveObjectTypes.Cylinder:
                    path = "PreviewCylinder";
                    break;
                case PrimitiveObjectTypes.Custom:
                    #if UNITY_EDITOR
                    path = UnityEditor.EditorUtility.OpenFilePanel("Select custom object", "", "fbx");
                    #endif
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(primitiveType), primitiveType, null);
            }

            return GameObject.Instantiate( Resources.Load<GameObject>(path));
        }
    }
}