using System;
using UnityEngine;

namespace Unity.Muse.Texture
{
    internal static class PrimitiveObjectsProvider
    {
        public static GameObject GetPrimitiveInstance(PrimitiveObjectTypes primitiveType)
        {
#if HDRP_PIPELINE_ENABLED
            return GetHdrpPrimitives(primitiveType);
#endif
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

        static GameObject GetHdrpPrimitives(PrimitiveObjectTypes primitiveObjectTypes)
        {
            var gameObjectParent = new GameObject("Preview Primitive");
            GameObject primitive = null;
            switch (primitiveObjectTypes)
            {
                case PrimitiveObjectTypes.Sphere:
                    primitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    break;
                case PrimitiveObjectTypes.Cube:
                    primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    break;
                case PrimitiveObjectTypes.Plane:
                    primitive = GameObject.CreatePrimitive(PrimitiveType.Plane);
                    primitive.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    break;
                case PrimitiveObjectTypes.Cylinder:
                    primitive = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    break;
                case PrimitiveObjectTypes.Custom:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(primitiveObjectTypes), primitiveObjectTypes, null);
            }
            if(primitive != null)
                primitive.transform.parent = gameObjectParent.transform;
            return gameObjectParent;
        }
    }
}