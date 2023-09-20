using System.IO;
using Unity.Muse.Common;
using UnityEditor;
using UnityEngine;

namespace Unity.Muse.Texture.Editor
{
    public class MaterialExporter
    {
        [InitializeOnLoadMethod]
        static void MaterialExportRegister()
        {
            ExportHandler.OnExportMaterialWithPrompt += OnExportMaterialWithPrompt;
            ExportHandler.OnExportMaterial += ExportMaterial;
        }

        static void OnExportMaterialWithPrompt(Artifact baseArtifact, ProcessedPbrMaterialData materialData)
        {
            var path = EditorUtility.SaveFilePanelInProject("Save material", baseArtifact.Guid, "mat", "");
            if (string.IsNullOrEmpty(path)) return;

            ExportMaterial(baseArtifact, materialData, path);
        }

        internal static void ExportMaterial(Artifact baseArtifact, ProcessedPbrMaterialData materialData, string path)
        {
            var material = new Material(MaterialGeneratorUtils.GetDefaultShaderForPipeline());

            var materialAsset = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (materialAsset == null)
            {
                materialAsset = new Material(material);
                AssetDatabase.CreateAsset(materialAsset, path);
            }

            var baseMap = SaveAndLoadTexture2D("baseMap", materialData.BaseMapPNGData, materialAsset, false, true);
            var diffuseMap = SaveAndLoadTexture2D("diffuseMap", materialData.DiffuseMapPNGData, materialAsset);
            var normalMap = SaveAndLoadTexture2D("normalMap", materialData.NormalMapPNGData, materialAsset, true);
            var metallicMap = SaveAndLoadTexture2D("metallicMap", materialData.MetallicMapPNGData, materialAsset);
            var roughnessMap = SaveAndLoadTexture2D("roughnessMap", materialData.RoughnessMapPNGData, materialAsset);
            var heightMap = SaveAndLoadTexture2D("heightMap", materialData.HeightmapPNGData, materialAsset);
            var aoMap = SaveAndLoadTexture2D("ambientOcclusionMap", materialData.AOMapPNGData, materialAsset);

            materialAsset.SetTexture(MuseMaterialProperties.baseMapKey, diffuseMap);
            materialAsset.SetTexture(MuseMaterialProperties.normalMapKey, normalMap);
            materialAsset.SetTexture(MuseMaterialProperties.metallicMapKey, metallicMap);
            materialAsset.SetTexture(MuseMaterialProperties.roughnessMapKey, roughnessMap);
            materialAsset.SetTexture(MuseMaterialProperties.heightMapKey, heightMap);
            materialAsset.SetTexture(MuseMaterialProperties.ambientOcclusionMapKey, aoMap);

#if !HDRP_PIPELINE_ENABLED
            materialAsset.SetFloat(MuseMaterialProperties.useDisplacement, 0f);
#endif

            CopyPbrMaterialProperty(materialAsset, baseArtifact);

            EditorUtility.SetDirty(materialAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(materialAsset));
        }

        static Texture2D SaveAndLoadTexture2D(string name, byte[] pngData, Material materialAsset, bool isNormalMap = false, bool isSRGB = false)
        {
            // Get path of the base asset
            var baseAssetPath = AssetDatabase.GetAssetPath(materialAsset);

            // Get directory path from the base asset path
            var baseAssetDir = Path.GetDirectoryName(baseAssetPath);

            var folderName = materialAsset.name + "_Maps";

            // Create the new folder's path
            var newFolderPath = Path.Combine(baseAssetDir, folderName);

            // Check if the directory already exists, if not, create it
            if (!AssetDatabase.IsValidFolder(newFolderPath))
            {
                AssetDatabase.CreateFolder(baseAssetDir, folderName);
            }

            // Prepare the path for the new PNG
            var pngPath = Path.Combine(newFolderPath, name  + ".png");

            // Save the PNG data to a file
            if (pngData is null)
                return new Texture2D(2,2);
            File.WriteAllBytes(pngPath, pngData);

            // Ensure the new asset is included in the AssetDatabase
            AssetDatabase.ImportAsset(pngPath, ImportAssetOptions.ForceUpdate);

            if (isNormalMap || !isSRGB)
            {
                var textureImporter = AssetImporter.GetAtPath(pngPath) as TextureImporter;

                // Create TextureImporterSettings
                var textureImporterSettings = new TextureImporterSettings();
                textureImporter.ReadTextureSettings(textureImporterSettings);

                if (isNormalMap)
                {
                    // Enable normal map
                    textureImporterSettings.textureType = TextureImporterType.NormalMap;
                    textureImporterSettings.convertToNormalMap = false;
                }

                textureImporterSettings.sRGBTexture = isSRGB;

                // Apply settings
                textureImporter.SetTextureSettings(textureImporterSettings);

                // Ensure the new asset is included in the AssetDatabase
                AssetDatabase.ImportAsset(pngPath, ImportAssetOptions.ForceUpdate);
            }

            // Load the texture from the saved PNG file
            var newTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(pngPath);

            return newTexture;
        }

        internal static void CopyPbrMaterialProperty(Material material, Artifact baseArtifact)
        {
            if (baseArtifact is not ImageArtifact imageArtifact) return;

            imageArtifact.MaterialMetaData ??= new ImageArtifact.MaterialData(true);
            material.SetFloat(MuseMaterialProperties.heightIntensity, imageArtifact.MaterialMetaData.height);
            material.SetFloat(MuseMaterialProperties.metallicIntensity, imageArtifact.MaterialMetaData.metallic);
            material.SetFloat(MuseMaterialProperties.roughnessIntensity, imageArtifact.MaterialMetaData.roughness);
        }
    }
}
