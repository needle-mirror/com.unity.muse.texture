using System;
using System.Collections.Generic;
using System.IO;
using Unity.Muse.Common;
using Unity.Muse.Common.Editor;
using Unity.Muse.Texture.Editor.Analytics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Scripting;

namespace Unity.Muse.Texture.Editor
{
    [Preserve]
    public sealed class PbrArtifactDragAndDropHandler : IArtifactDragAndDropHandler
    {
        ImageArtifact m_BaseMap;
        ImageArtifact m_MetallicMapSourceArtifact;
        ImageArtifact m_HeightmapSourceArtifact;
        ImageArtifact m_NormalMapSourceArtifact;
        ImageArtifact m_RoughnessMapSourceArtifact;

        public static void Register()
        {
            DragAndDropFactory.SetHandlerForArtifact("PBR Material", typeof(PbrArtifactDragAndDropHandler));
        }

        public PbrArtifactDragAndDropHandler(IList<Artifact> imageArtifacts)
        {
            Debug.Assert(imageArtifacts != null && imageArtifacts.Count == 5);

            m_BaseMap = (ImageArtifact)imageArtifacts[0];
            m_MetallicMapSourceArtifact = (ImageArtifact)imageArtifacts[1];
            m_HeightmapSourceArtifact = (ImageArtifact)imageArtifacts[2];
            m_NormalMapSourceArtifact = (ImageArtifact)imageArtifacts[3];
            m_RoughnessMapSourceArtifact = (ImageArtifact)imageArtifacts[4];

            Debug.Assert(m_BaseMap != null);
            Debug.Assert(m_MetallicMapSourceArtifact != null);
            Debug.Assert(m_HeightmapSourceArtifact != null);
            Debug.Assert(m_NormalMapSourceArtifact != null);
            Debug.Assert(m_RoughnessMapSourceArtifact != null);
        }

        bool EvaluateDropTarget(GameObject go)
        {
            return !(go == null || go.GetComponent<MeshRenderer>() == null);
        }

        public bool CanDropSceneView(GameObject dropUpon, Vector3 worldPosition)
        {
            return EvaluateDropTarget(dropUpon);
        }

        public void HandleDropSceneView(GameObject dropUpon, Vector3 worldPosition)
        {
            Model.SendAnalytics(new SaveTextureData {is_pbr_material = true, material_hash = ""});
            GetProcessedMaterialData((materialData) =>
            {
                DropOnGameObject(dropUpon, worldPosition, materialData);
            });
        }

        void DropOnGameObject(GameObject go, Vector3 worldPosition, ProcessedPbrMaterialData materialData)
        {
            Model.SendAnalytics(new SaveTextureData {is_pbr_material = true, material_hash = ""});
            if (!EvaluateDropTarget(go))
            {
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(go, $"Add PBR material ({go.name})");

            var meshRenderer = go.GetComponent<MeshRenderer>();

            var material = new Material(MaterialGeneratorUtils.GetDefaultShaderForPipeline());
            MaterialGeneratorUtils.CreateTexturesAndMaterialForRP(materialData, material, true);

            MaterialExporter.CopyPbrMaterialProperty(material, m_BaseMap);
            
#if !HDRP_PIPELINE_ENABLED
            material.SetFloat(MuseMaterialProperties.useDisplacement, 0f);
#endif
            meshRenderer.sharedMaterial = material;
        }

        public bool CanDropHierarchy(GameObject dropUpon)
        {
            return EvaluateDropTarget(dropUpon);
        }

        public void HandleDropHierarchy(GameObject dropUpon)
        {
            Model.SendAnalytics(new SaveTextureData {is_pbr_material = true, material_hash = ""});
            HandleDropSceneView(dropUpon, Vector3.zero);
        }

        public bool CanDropProject(string path)
        {
            return true;
        }

        public void HandleDropProject(string path)
        {
            Model.SendAnalytics(new SaveTextureData {is_pbr_material = true, material_hash = ""});

            path = GetPathRelativeToRoot(path);
            if (string.IsNullOrWhiteSpace(path))
                path = "Assets";
            path = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(path, m_BaseMap.Guid + ".mat"));

            GetProcessedMaterialData((materialdata) =>
            {
                MaterialExporter.ExportMaterial(m_BaseMap, materialdata, path);  
            });

        }

        public void GetProcessedMaterialData(Action<ProcessedPbrMaterialData> onDone)
        {
           var materialData = new ProcessedPbrMaterialData();

            bool EvaluateMapsCompleteness()
            {
                if (materialData.BaseMapPNGData == null)
                    return false;
                if (materialData.HeightmapPNGData == null)
                    return false;
                if (materialData.MetallicMapPNGData == null)
                    return false;
                if (materialData.NormalMapPNGData == null)
                    return false;
                if (materialData.RoughnessMapPNGData == null)
                    return false;

                return true;
            }

            m_BaseMap.GetArtifact((artifactInstance, rawData, errorMessage) =>
            {
                materialData.BaseMapPNGData = rawData;
                if(EvaluateMapsCompleteness())
                    onDone?.Invoke(materialData);
            }, true);
            m_HeightmapSourceArtifact.GetArtifact((artifactInstance, rawData, errorMessage) =>
            {
                materialData.HeightmapPNGData = rawData;
                if(EvaluateMapsCompleteness())
                    onDone?.Invoke(materialData);
            }, true);
            m_NormalMapSourceArtifact.GetArtifact((artifactInstance, rawData, errorMessage) =>
            {
                materialData.NormalMapPNGData = rawData;
                if (EvaluateMapsCompleteness())
                    onDone?.Invoke(materialData);
            }, true);
            m_MetallicMapSourceArtifact.GetArtifact((artifactInstance, rawData, errorMessage) =>
            {
                materialData.MetallicMapPNGData = rawData;
                if(EvaluateMapsCompleteness())
                    onDone?.Invoke(materialData);
            }, true);
            m_RoughnessMapSourceArtifact.GetArtifact((artifactInstance, rawData, errorMessage) =>
            {
                materialData.RoughnessMapPNGData = rawData;
                if(EvaluateMapsCompleteness())
                    onDone?.Invoke(materialData);
            }, true);
        }

        static string GetPathRelativeToRoot(string path)
        {
            var pathStartIndex = path.IndexOf("Assets");
            return pathStartIndex == -1 ? string.Empty : path.Substring(pathStartIndex);
        }
    }
}
