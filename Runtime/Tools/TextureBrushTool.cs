using Unity.Muse.Common;
using Unity.Muse.Common.Tools;

namespace Unity.Muse.Texture.Tools
{
    internal class TextureBrushTool: BrushTool<MaskOperator>
    {
        protected override void Initialize()
        {
            base.Initialize();
            
            m_CurrentModel.OnActiveToolChanged -= OnActiveToolChanged;
            m_CurrentModel.OnActiveToolChanged += OnActiveToolChanged;
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
            {
                m_CurrentModel.SetOperatorEnable<MaskOperator>(false); 
            }
        }

        public override bool EvaluateEnableState(Artifact artifact)
        {
            if (artifact is ImageArtifact imageArtifact)
            {
                return m_CurrentModel.isRefineMode && !imageArtifact.IsPbrMode && ArtifactCache.IsInCache(artifact);
            }
            
            return base.EvaluateEnableState(artifact);
        }
    }
}