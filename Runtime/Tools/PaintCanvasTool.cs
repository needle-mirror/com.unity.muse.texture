using System.Collections.Generic;
using System.Linq;
using Unity.Muse.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Texture
{
    public class PaintCanvasTool: ICanvasTool
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
            model.OnSetMaskSeamless += OnSetMaskSeamless;
            model.OnActiveToolChanged += OnActiveToolChanged;
        }

        void OnActiveToolChanged(ICanvasTool obj)
        {
            // Clear reference image when going to in-painting mask as they are currently exclusive
            if (obj is not null)
                m_CurrentModel.RemoveOperators(m_CurrentModel.CurrentOperators.GetOperator<ReferenceOperator>() ?? new());
            else
            {
                // Clear in-painting mask when setting a new reference image as they are currently exclusive
                if (m_CurrentModel.CurrentOperators.GetOperator<ReferenceOperator>() is not null)
                {
                    m_CurrentModel.CurrentOperators.GetOperator<MaskOperator>()?.Enable(false);
                    m_CurrentModel.UpdateOperators();
                }
            }
        }

        public bool EvaluateEnableState(Artifact artifact)
        {
            return m_CurrentModel.isRefineMode && (m_CurrentModel.CurrentOperators?.Any(x => x is MaskOperator) ?? false);
        }

        public void ActivateOperators()
        {
            if(m_CurrentModel == null) return;

            var opMask = m_CurrentModel.CurrentOperators.Find(x => x.GetType() == typeof(MaskOperator));
            if (opMask != null && !opMask.Enabled())
            {
                opMask.Enable(true);
                //Probably need to do an event refresh nodes list
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
            return m_CurrentManipulator?.m_PaintingManipulatorSettings.GetSettings();
        }

        public Texture2D Export()
        {
            return m_CurrentManipulator.Export().ToTexture2D();
        }

        class PaintCanvasToolManipulator : CanvasManipulator
        {
            PaintingManipulator m_PaintingManipulator;
            public PaintingManipulatorSettings m_PaintingManipulatorSettings;
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
                m_PaintingManipulatorSettings = new PaintingManipulatorSettings(m_PaintingManipulator);
                m_PaintingManipulator.SetModel(m_CurrentModel);
                node.PaintSurfaceElement.AddManipulator(m_PaintingManipulator);
            }

            protected override void UnregisterCallbacksFromTarget()
            {
                m_PaintingManipulator?.target.RemoveManipulator(m_PaintingManipulator);
            }

            public RenderTexture Export()
            {
                return m_PaintingManipulator?.GetTexture();
            }
        }
    }
}
