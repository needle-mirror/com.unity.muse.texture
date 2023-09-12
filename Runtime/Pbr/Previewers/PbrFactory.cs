using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Muse.Common;
using UnityEngine;

namespace Unity.Muse.Texture
{
    public class PbrFactory
    {
        public event Action<Material, ProcessedPbrMaterialData> OnMaterialCreated;
        public event Action<float> OnProgress;

        PbrMapGeneratorJob m_CurrentMapGenerationJob;
        readonly List<PbrMaterialData> m_PbrMaterialData;

        public PbrFactory(List<PbrMaterialData> viewModel)
        {
            m_PbrMaterialData = viewModel;
        }

        public void StartPbrMapCreation(ImageArtifact forArtifact)
        {
            if (m_CurrentMapGenerationJob != null && m_CurrentMapGenerationJob.BaseMapArtifact == forArtifact)
            {
                return;
            }

            CancelCurrentGeneration();

            var data = m_PbrMaterialData.Find(x => x.BaseMapSourceArtifact.Guid == forArtifact.Guid);

            if (data == null)
            {
                data = new PbrMaterialData
                {
                    BaseMapSourceArtifact = forArtifact
                };

                m_PbrMaterialData.Add(data);
            }

            m_CurrentMapGenerationJob = new PbrMapGeneratorJob(forArtifact,
                data.HeightmapSourceArtifact,
                data.NormalMapSourceArtifact,
                data.MetallicMapSourceArtifact,
                data.RoughnessMapSourceArtifact);


            OnProgress?.Invoke(0f);
            m_CurrentMapGenerationJob.Completed += OnPBRMapRequestCompleted;
            m_CurrentMapGenerationJob.ProgressUpdated += OnPBRJobProgressChanged;

            // We need to wait for the image generation to complete in the back end if it wasn't already.
            // So Only start the job once we confirmed the source artifact is available
            forArtifact.GetArtifact((_, _, error) =>
            {
                if (string.IsNullOrEmpty(error))
                {
                    OnImageGenerationConfirmed(forArtifact);
                }
                else
                {
                    Debug.LogError($"Failed to start PBR job because the base map generation failed: {error}");
                }
            }, true);
        }

        void OnImageGenerationConfirmed(Artifact artifact)
        {
            if (m_CurrentMapGenerationJob is { IsRunning: false }
                && artifact == m_CurrentMapGenerationJob.BaseMapArtifact)
            {
                m_CurrentMapGenerationJob.Start();
            }
        }

        void OnPBRMapRequestCompleted(bool success, ProcessedPbrMaterialData materialData)
        {
            if (!success)
            {
                OnProgress?.Invoke(100f);
                return;
            }

            foreach (var data in m_PbrMaterialData.Where(data => data.BaseMapSourceArtifact.Guid == materialData.BaseMap.Guid))
            {
                data.MetallicMapSourceArtifact = materialData.MetallicMap;
                data.HeightmapSourceArtifact = materialData.HeightmapMap;
                data.NormalMapSourceArtifact = materialData.NormalMap;
                data.RoughnessMapSourceArtifact = materialData.RoughnessMap;
            }

            var newMaterial = new Material(MaterialGeneratorUtils.GetDefaultShaderForPipeline());

            MaterialGeneratorUtils.CreateTexturesAndMaterialForRP(materialData, newMaterial, false);
            
            OnMaterialCreated?.Invoke(newMaterial, materialData);
            CancelCurrentGeneration();
        }
        
        /// <summary>
        /// Saves the Texture2D as a PNG.
        /// </summary>
        /// <param name="texture">The texture to save.</param>
        /// <param name="fileName">Name of the file without extension.</param>
        /// <param name="directoryPath">Directory path to save the texture in. Uses Application.persistentDataPath if not provided.</param>
        public static void SaveTextureAsPNG(Texture2D texture, string fileName, string directoryPath = null)
        {
            if (texture == null)
            {
                Debug.LogError("Texture is null. Cannot save.");
                return;
            }

            if (string.IsNullOrEmpty(fileName))
            {
                Debug.LogError("FileName is null or empty. Cannot save.");
                return;
            }

            // Use the persistent data path if no path is provided.
            if (string.IsNullOrEmpty(directoryPath))
            {
                directoryPath = Application.persistentDataPath;
            }

            string filePath = Path.Combine(directoryPath, fileName + ".png");
        
            byte[] pngBytes = texture.EncodeToPNG();
            File.WriteAllBytes(filePath, pngBytes);

            Debug.Log($"Saved texture to: {filePath}");
        }

        void OnPBRJobProgressChanged(float obj)
        {
            OnProgress?.Invoke(obj * 100f);
        }

        public void CancelCurrentGeneration()
        {
            if (m_CurrentMapGenerationJob != null)
            {
                m_CurrentMapGenerationJob.Completed -= OnPBRMapRequestCompleted;
                m_CurrentMapGenerationJob.Cancel();
                m_CurrentMapGenerationJob = null;
            }
        }
    }
}