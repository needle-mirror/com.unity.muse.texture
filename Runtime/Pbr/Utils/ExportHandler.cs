using System;
using Unity.Muse.Common;

namespace Unity.Muse.Texture
{
    /// <summary>
    /// Export handler for texture
    /// </summary>
    internal static class ExportHandler
    {
        /// <summary>
        /// Event invoked when a material is exported with prompt
        /// </summary>
        public static event Action<Artifact, ProcessedPbrMaterialData> OnExportMaterialWithPrompt;
        
        /// <summary>
        /// Event invoked when a material is exported without prompt
        /// </summary>
        public static event Action<Artifact, ProcessedPbrMaterialData, string> OnExportMaterial;
        
        internal static void ExportWithPrompt(Artifact artifact, ProcessedPbrMaterialData materialData)
        {
            OnExportMaterialWithPrompt?.Invoke(artifact, materialData);
        }
        
        internal static void ExportWithoutPrompt(Artifact baseArtifact, ProcessedPbrMaterialData materialData, string path)
        {
            OnExportMaterial?.Invoke(baseArtifact, materialData, path); 
        }
    }
}
