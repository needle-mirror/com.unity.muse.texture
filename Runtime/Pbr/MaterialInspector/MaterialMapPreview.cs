using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Texture
{
    internal class MaterialMapPreview: MaterialPreviewElement
    {
        public new class UxmlFactory : UxmlFactory<MaterialMapPreview, UxmlTraits> { }
        
        private MaterialPreviewItem m_SelectedPreviewItem;
        private UnityEngine.Texture m_MaterialPreviewTexture;
        
        PrimitiveObjectTypes m_CurrentPreviewType = PrimitiveObjectTypes.Sphere;
        HdriEnvironment m_CurrentHdriEnvironment = HdriEnvironment.Default;
        
        internal PrimitiveObjectTypes CurrentPreviewType => m_CurrentPreviewType;
        internal HdriEnvironment CurrentHdriEnvironment => m_CurrentHdriEnvironment;
        
        public MaterialMapPreview()
        {
           m_ModifierRotation = false;
           m_MaterialPreviewTexture = s_MaterialPreviewer.CreateDefaultRenderTexture();
        }

        public new void SetMaterial(Material material)
        {
            base.SetMaterial(material);
        }
        
        public void SetSelectedPreviewItem(MaterialPreviewItem previewItem)
        {
            m_SelectedPreviewItem = previewItem;
            Render(Vector2.zero);
        }
        
        public void SetPreviewType(PrimitiveObjectTypes previewType)
        {
            m_CurrentPreviewType = previewType;
            RefreshRender(); 
        }

        internal void SetHdriEnvironment(HdriEnvironment environment)
        {
            m_CurrentHdriEnvironment = environment;
            RefreshRender();
        }

        internal override void Render(Vector2 dragValue, PrimitiveObjectTypes previewType = PrimitiveObjectTypes.Sphere, HdriEnvironment environment = HdriEnvironment.Default)
        {
            switch (m_SelectedPreviewItem)
            {
                case MaterialPreviewItem.Material:
                    tooltip = "Shift + Drag to rotate the model.";
                    m_PreviewImage.image = m_MaterialPreviewTexture;
                    base.Render(dragValue, m_CurrentPreviewType, m_CurrentHdriEnvironment);
                    break;
                case MaterialPreviewItem.Artifact:
                case MaterialPreviewItem.BaseMap:
                    RenderMap(MuseMaterialProperties.baseMapKey);
                    break;
                case MaterialPreviewItem.NormalMap:
                    RenderMap(MuseMaterialProperties.normalMapKey);
                    break;
                case MaterialPreviewItem.MetallicMap:
                    RenderMap(MuseMaterialProperties.metallicMapKey);
                    break;
                case MaterialPreviewItem.RoughnessMap:
                    RenderMap(MuseMaterialProperties.roughnessMapKey);
                    break;
                case MaterialPreviewItem.HeightMap:
                    RenderMap(MuseMaterialProperties.heightMapKey);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        void RenderMap(int propertyId)
        {
            tooltip = "";
            if(m_Material == null)
                return;
            
            m_PreviewImage.image = m_Material.GetTexture(propertyId);
        }
    }
}