using System.Linq;
using Unity.Jobs;
using Unity.Muse.Common;
using UnityEngine;
using UnityEngine.Rendering;

namespace Unity.Muse.Texture
{
    public static partial class MaterialGeneratorUtils
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="materialData"></param>
        /// <param name="targetMaterial"></param>
        /// <param name="convertNormalsToDxtNm">Will convert the loaded artifact for this material's bumpmap directly to DXTNm instead of relying on unity's importer. Use for runtime previews only.</param>
        public static void CreateTexturesAndMaterialForRP(ProcessedPbrMaterialData materialData,
            Material targetMaterial, bool convertNormalsToDxtNm = true, object context = null)
        {
            var baseMap = new Texture2D(2, 2, TextureFormat.RGBA32, true, false);
            baseMap.LoadImage(materialData.DiffuseMapPNGData);
            ObjectUtils.Retain(baseMap, context);
            targetMaterial.SetTexture(MuseMaterialProperties.baseMapKey, baseMap);

            var normalMap = new Texture2D(2, 2, TextureFormat.RGBA32, true, true);
            ObjectUtils.Retain(normalMap, context);
            normalMap.LoadImage(materialData.NormalMapPNGData);
            if (convertNormalsToDxtNm)
            {
                var oldNormalMap = normalMap;
                normalMap = ConvertToDxtNormalMap(oldNormalMap);
                ObjectUtils.Retain(normalMap, context);
                oldNormalMap.SafeDestroy();
            }

            targetMaterial.SetTexture(MuseMaterialProperties.normalMapKey, normalMap);

            var metallicMap = new Texture2D(2, 2, TextureFormat.RGBA32, true, true);
            ObjectUtils.Retain(metallicMap, context);
            metallicMap.LoadImage(materialData.MetallicMapPNGData);
            targetMaterial.SetTexture(MuseMaterialProperties.metallicMapKey, metallicMap);

            var roughnessMap = new Texture2D(2, 2, TextureFormat.RGBA32, true, true);
            ObjectUtils.Retain(roughnessMap, context);
            roughnessMap.LoadImage(materialData.RoughnessMapPNGData);
            targetMaterial.SetTexture(MuseMaterialProperties.roughnessMapKey, roughnessMap);

            var heighMap = new Texture2D(2, 2, TextureFormat.RGBA32, true, true);
            ObjectUtils.Retain(heighMap, context);
            heighMap.LoadImage(materialData.HeightmapPNGData);
            targetMaterial.SetTexture(MuseMaterialProperties.heightMapKey, heighMap);
            
            var aoMap = new Texture2D(2, 2, TextureFormat.RGBA32, true, true);
            aoMap.LoadImage(materialData.AOMapPNGData);
            targetMaterial.SetTexture(MuseMaterialProperties.ambientOcclusionMapKey, aoMap); 
        }

        public static Shader GetDefaultShaderForPipeline()
        {
            return Resources.Load<Shader>("MuseMaterialShaderGraph");
        }

        /// <summary>
        /// Converts the incoming bumpmap texture into the format expected by vanilla unity shaders using DXTNm. Only use for runtime previewing of bumpmaps.
        /// Do not save the results to disk as is, and this is a lossy one way process
        /// </summary>
        /// <param name="incoming"></param>
        /// <returns></returns>
        static Texture2D ConvertToDxtNormalMap(Texture2D incoming)
        {
            Texture2D normalTexture = new Texture2D(incoming.width, incoming.height, TextureFormat.RGBA32, true, true);
            normalTexture.filterMode = FilterMode.Trilinear;
            normalTexture.wrapMode = TextureWrapMode.Repeat;

            ConvertTextToDxtnmJob job = new ConvertTextToDxtnmJob()
            {
                IncomingPixels = incoming.GetPixelData<Color32>(0),
                OutputPixels = normalTexture.GetPixelData<Color32>(0)
            };

            var handle = job.Schedule(job.IncomingPixels.Length, 32);
            handle.Complete();

            normalTexture.SetPixelData(job.OutputPixels, 0);
            normalTexture.Apply();

            return normalTexture;
        }
        
        /// <summary>
        /// Generate an Ambient Occlusion map from a heightmap byte[]
        /// </summary>
        /// <param name="heightMap"></param>
        /// <returns></returns>
        public static Texture2D GenerateAOMap(byte[] heightMap)
        {
            var heighMap = new Texture2D(2, 2, TextureFormat.RGBA32, true, false);
            heighMap.LoadImage(heightMap);

            var returnTexture = GenerateAOMap(heighMap);
            heighMap.SafeDestroy();
            
            return returnTexture;
        } 
        
        /// <summary>
        /// Generate an Ambient Occlusion map from a heightmap byte[]
        /// </summary>
        /// <param name="heightMap"></param>
        /// <returns></returns>
        public static Texture2D GenerateAOMap(Texture2D heightMap)
        {
            var shader = Resources.Load<Shader>("AO");
            var blitMaterial = new Material(shader);
            
            var heightmapRT = RenderTexture.GetTemporary(heightMap.width, heightMap.height, 0, RenderTextureFormat.R16, RenderTextureReadWrite.Linear);
            Graphics.Blit(heightMap, heightmapRT);
            
            var destRT = RenderTexture.GetTemporary(heightMap.width, heightMap.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            Graphics.Blit(heightmapRT, destRT, blitMaterial);

            var activeRT = RenderTexture.active;
            RenderTexture.active = destRT;
            
            var aoMap = new Texture2D(heightMap.width, heightMap.height, TextureFormat.RGBA32, true, true);
            aoMap.ReadPixels(new Rect(0, 0, aoMap.width, aoMap.height), 0, 0);
            aoMap.Apply();
            
            RenderTexture.active = activeRT;
            RenderTexture.ReleaseTemporary(destRT);

            return aoMap;
        }
    }
}