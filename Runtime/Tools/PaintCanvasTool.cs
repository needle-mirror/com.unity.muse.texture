using System.Collections.Generic;
using Unity.Muse.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Texture
{
    internal class PaintCanvasTool: ICanvasTool
    {
        Model m_CurrentModel;
        PaintCanvasToolManipulator m_CurrentManipulator;
        bool Seamless { get; set; }

        void OnSetMaskSeamless(bool value)
        {
            Seamless = value;
            m_CurrentManipulator?.OnSetMaskSeamless(Seamless);
        }

        public CanvasManipulator GetToolManipulator()
        {
            m_CurrentManipulator =new PaintCanvasToolManipulator(m_CurrentModel, Seamless);
            return m_CurrentManipulator;
        }

        public void SetModel(Model model)
        {
            m_CurrentModel = model;

            model.OnSetMaskSeamless -= OnSetMaskSeamless;
            model.OnActiveToolChanged -= OnActiveToolChanged;
            model.OnOperatorUpdated -= Refresh;

            model.OnSetMaskSeamless += OnSetMaskSeamless;
            model.OnActiveToolChanged += OnActiveToolChanged;
            model.OnOperatorUpdated += Refresh;
        }

        void Refresh(IEnumerable<IOperator> operators, bool set)
        {
            if (!m_CurrentModel.isRefineMode)
                return;
            if (m_CurrentManipulator is null)
                return;
            if (set)
                return;

            m_CurrentManipulator.RefreshMask();
        }

        void OnActiveToolChanged(ICanvasTool obj)
        {
            if (obj is not null)
                // Disable reference image when going to in-painting mask as they are currently exclusive
                m_CurrentModel.SetOperatorEnable<ReferenceOperator>(false);
            else if (m_CurrentModel.CurrentOperators.GetOperator<ReferenceOperator>() is not null)
            {
                // Disable in-painting mask when setting a new reference image as they are currently exclusive
                m_CurrentModel.SetOperatorEnable<MaskOperator>(false);
                m_CurrentModel.SetOperatorEnable<ReferenceOperator>(true);
            }

            // Disable in-painting mask when going out of paint mode
            if (obj is null)
                m_CurrentModel.SetOperatorEnable<MaskOperator>(false);
        }

        public bool EvaluateEnableState(Artifact artifact)
        {
            if (artifact is ImageArtifact imageArtifact)
            {
                return m_CurrentModel.isRefineMode && !imageArtifact.IsPbrMode && ArtifactCache.IsInCache(artifact);
            }
            return m_CurrentModel.isRefineMode;
        }

        public void ActivateOperators()
        {
            if(m_CurrentModel == null) return;

            var opMask = m_CurrentModel.CurrentOperators.Find(x => x.GetType() == typeof(MaskOperator)) ??
                m_CurrentModel.AddOperator<MaskOperator>();

            if (opMask != null && !opMask.Enabled())
            {
                opMask.Enable(true);
                m_CurrentModel.UpdateOperators(opMask);
            }
        }

        public ICanvasTool.ToolButtonData GetToolData()
        {
            return new ICanvasTool.ToolButtonData()
            {
                Name = "muse-pencil-tool-button",
                Label = "",
                Icon = "paint-brush",
                Tooltip = "Mask the area that you want to refine"
            };
        }

        public VisualElement GetSettings()
        {
            return m_CurrentManipulator?.paintingManipulatorSettings.GetSettings();
        }

        public Texture2D Export()
        {
            return m_CurrentManipulator.Export().ToTexture2D();
        }

        class PaintCanvasToolManipulator : CanvasManipulator
        {
            PaintingManipulator m_PaintingManipulator;
            internal PaintingManipulatorSettings paintingManipulatorSettings;
            bool Seamless { get; set; }

            public PaintCanvasToolManipulator(Model model, bool seamless)
                : base(model)
            {
                Seamless = seamless;
            }

            public void OnSetMaskSeamless(bool value)
            {
                Seamless = value;
                m_PaintingManipulator?.SetMaskSeamless(Seamless);
            }

            protected override void RegisterCallbacksOnTarget()
            {
                var nodeQuery = target.Query<ArtifactView>().Where((element) => element.Artifact.Guid == m_CurrentModel.SelectedArtifact.Guid);
                var node = nodeQuery.First();

                if (node == null)
                {
                    Debug.LogError("Could not find Node");
                    return;
                }

                m_PaintingManipulator = new PaintingManipulator(Seamless, true);
                paintingManipulatorSettings = new PaintingManipulatorSettings(m_PaintingManipulator);
                m_PaintingManipulator.SetModel(m_CurrentModel);
                node.PaintSurfaceElement.AddManipulator(m_PaintingManipulator);
                RefreshMask();
            }

            protected override void UnregisterCallbacksFromTarget()
            {
                m_PaintingManipulator?.target.RemoveManipulator(m_PaintingManipulator);
            }

            public RenderTexture Export()
            {
                return m_PaintingManipulator?.GetTexture();
            }

            public void RefreshMask()
            {
                var maskOperator = m_CurrentModel.CurrentOperators.GetOperator<MaskOperator>();
                if (maskOperator is null)
                    return;
                m_PaintingManipulator.paintingElement?.SetMaskTexture(maskOperator.GetMask());
            }
        }
    }
}

