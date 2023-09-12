using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Muse.Common;
using Unity.Muse.Texture.Pbr.Cache;
using UnityEngine.UIElements;

namespace Unity.Muse.Texture
{
    internal class PreviewPbrArtifact : MaterialMapPreview, IDisposable
    {
        public Material CurrentMaterial => m_CurrentMaterial;
        public ProcessedPbrMaterialData CurrentMaterialData => m_CurrentMaterialData;
        public bool PreviewEnabled => m_PreviewEnabled;
        public event Action<ImageArtifact> OnArtifactSelected;

        public readonly GenericLoader GenericLoader;
        readonly PbrFactory m_PbrFactory;

        ImageArtifact m_CurrentArtifact;

        Material m_CurrentMaterial;
        ProcessedPbrMaterialData m_CurrentMaterialData;
        
        public GenericLoader.State LoadingState => GenericLoader.LoadingState;

        public PreviewPbrArtifact(List<PbrMaterialData> viewModel, bool autoLoading = true) : base()
        {
            m_PreviewEnabled = false;
            RegisterCallback<PointerDownEvent>(OnSelected);

            m_PbrFactory = new PbrFactory(viewModel);
            m_PbrFactory.OnMaterialCreated += OnMaterialCreated;
            m_PbrFactory.OnProgress += PbrGenerationProgress;

            style.overflow = Overflow.Hidden;

            GenericLoader = new GenericLoader(autoLoading ? GenericLoader.State.Loading : GenericLoader.State.None, true)
            {
                style =
                {
                    backgroundColor = new Color(0.1568628f, 0.1568628f, 0.1568628f, 1f),
                    position = Position.Absolute,
                    width = Length.Percent(100),
                    height = Length.Percent(100)
                }
            };

            Add(GenericLoader);
        }

        public void SetLoadingState(bool enabled)
        {
            GenericLoader.SetState(enabled ? GenericLoader.State.Loading : GenericLoader.State.None);
            GenericLoader.style.display = enabled ? DisplayStyle.Flex : DisplayStyle.None;
            MarkDirtyRepaint();
        }

        public void RenderFrameAndRepaint()
        {
            MarkDirtyRepaint();
        }

        public void SetAsset(Artifact artifact)
        {
            if(artifact is not ImageArtifact imageArtifact) return;

            m_CurrentArtifact = imageArtifact;
            
            m_CurrentArtifact.MaterialMetaData ??= new ImageArtifact.MaterialData(true);

            SetLoadingState(true);
            if (PbrDataCache.IsInCache(artifact))
            {
                GetCachedMaterial(artifact);
            }
            else
            {
               m_PbrFactory.StartPbrMapCreation(m_CurrentArtifact);
            }
        }

        public void Enable()
        {
            m_PreviewEnabled = true;
        }

        public void Disable()
        {
            m_PreviewEnabled = false;
        }

        void OnMaterialCreated(Material material, ProcessedPbrMaterialData materialData)
        {
            Dispose();

            m_CurrentMaterial = material;
            m_CurrentMaterialData = materialData;

            SetMaterial(m_CurrentMaterial);
            SetLoadingState(false);
        }

        public void Dispose()
        {
            m_PbrFactory?.CancelCurrentGeneration();
            if (m_CurrentMaterial == null) return;

            m_CurrentMaterial.SafeDestroy();
        }

        void PbrGenerationProgress(float progress)
        {
            GenericLoader.SetProgress(progress);
        }

        void OnSelected(PointerDownEvent evt)
        {
            if(evt.clickCount == 2 && evt.button == 0) //Double click
                OnArtifactSelected?.Invoke(m_CurrentArtifact);
        }

        void GetCachedMaterial(Artifact artifact)
        {
            var processedPbrMaterialData = PbrDataCache.GetPbrMaterialData(artifact);
            var newMaterial = new Material(MaterialGeneratorUtils.GetDefaultShaderForPipeline());
            MaterialGeneratorUtils.CreateTexturesAndMaterialForRP(processedPbrMaterialData, newMaterial);
            OnMaterialCreated(newMaterial, processedPbrMaterialData);
        }
    }
}
