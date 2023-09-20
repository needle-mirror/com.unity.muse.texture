using System;
using Unity.Muse.Common;
using UnityEngine;

namespace Unity.Muse.Texture
{
    [Serializable]
    public class PbrMaterialData
    {
        [SerializeField]
        public ImageArtifact BaseMapSourceArtifact;
        [SerializeField]
        public ImageArtifact NormalMapSourceArtifact;
        [SerializeField]
        public ImageArtifact MetallicMapSourceArtifact;
        [SerializeField]
        public ImageArtifact RoughnessMapSourceArtifact;
        [SerializeField]
        public ImageArtifact HeightmapSourceArtifact;
        [SerializeField]
        public ImageArtifact DiffuseMapSourceArtifact;

        public PbrMaterialData(){}
        public PbrMaterialData(ProcessedPbrMaterialData processedData)
        {
            BaseMapSourceArtifact = processedData.BaseMap;
            NormalMapSourceArtifact = processedData.NormalMap;
            MetallicMapSourceArtifact = processedData.MetallicMap;
            RoughnessMapSourceArtifact = processedData.RoughnessMap;
            HeightmapSourceArtifact = processedData.HeightmapMap;
        }
    }

    public struct ProcessedPbrMaterialData
    {
        public static readonly ProcessedPbrMaterialData k_FailedData = new ProcessedPbrMaterialData()  {
                                                                                            BaseMap = null,
                                                                                            BaseMapPNGData = null,
                                                                                            NormalMap = null,
                                                                                            NormalMapPNGData = null,
                                                                                            MetallicMap = null,
                                                                                            MetallicMapPNGData = null,
                                                                                            RoughnessMap = null,
                                                                                            RoughnessMapPNGData = null,
                                                                                            HeightmapMap = null,
                                                                                            HeightmapPNGData = null,
                                                                                            AOMapPNGData = null,
                                                                                            DiffuseMap = null,
                                                                                            DiffuseMapPNGData = null
                                                                                        };

        public ImageArtifact BaseMap;
        public byte[] BaseMapPNGData;
        public ImageArtifact DiffuseMap;
        public byte[] DiffuseMapPNGData;
        public ImageArtifact NormalMap;
        public byte[] NormalMapPNGData;
        public ImageArtifact MetallicMap;
        public byte[] MetallicMapPNGData;
        public ImageArtifact RoughnessMap;
        public byte[] RoughnessMapPNGData;
        public ImageArtifact HeightmapMap;
        public byte[] HeightmapPNGData;

        public byte[] AOMapPNGData
        {
            get
            {
                if (HeightmapPNGData == null)
                    return null;

                if (m_AOMapPNGData != null) return m_AOMapPNGData;
                
                var texture = MaterialGeneratorUtils.GenerateAOMap(HeightmapPNGData);
                m_AOMapPNGData = texture.EncodeToPNG();
                texture.SafeDestroy();

                return m_AOMapPNGData;
            }
            set => m_AOMapPNGData = value;
        }

        private byte[] m_AOMapPNGData;
    }
}
