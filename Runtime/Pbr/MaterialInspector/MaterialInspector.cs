using System.Collections;
using System.Collections.Generic;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Texture
{
    internal class MaterialInspector : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<MaterialInspector, UxmlTraits>
        {
        }

        [SerializeField]
        private Material m_Material;
        [SerializeField]
        private MaterialPreviewItem m_SelectedItem;

        private MaterialInspectorView m_MaterialInspectorView; 
        private MaterialMapPreview m_MaterialMapPreview;
        private MaterialPreviewSelector m_MaterialPreviewSelector;
        private MaterialPreviewSettings m_MaterialPreviewSettings;
        
        public MaterialInspector()
        {
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            m_MaterialInspectorView = this.Q<MaterialInspectorView>("MaterialInspectorView");
            m_MaterialMapPreview = this.Q<MaterialMapPreview>("MainPreview");
            m_MaterialPreviewSelector = this.Q<MaterialPreviewSelector>("PreviewSelector");
            m_MaterialPreviewSettings = this.Q<MaterialPreviewSettings>("PreviewSettings");
            
            if(m_Material != null)
                SetMaterial(m_Material);
            
            m_MaterialPreviewSelector.SelectItem(m_SelectedItem);
            m_MaterialMapPreview.SetSelectedPreviewItem(m_SelectedItem);
            
            m_MaterialPreviewSelector.OnPreviewSelected += OnMaterialMaterialPreviewSelected;
            m_MaterialInspectorView.OnMaterialPropertiesChanged += OnMaterialPropertiesChanged;
            m_MaterialPreviewSettings.OnTargetPrimitiveChanged += OnMaterialMapPreviewSelected;
            m_MaterialPreviewSettings.OnHdriChanged += OnHdriEnvironmentChanged;
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            m_MaterialPreviewSelector.OnPreviewSelected -= OnMaterialMaterialPreviewSelected;
            m_MaterialInspectorView.OnMaterialPropertiesChanged -= OnMaterialPropertiesChanged;
            m_MaterialPreviewSettings.OnTargetPrimitiveChanged -= OnMaterialMapPreviewSelected;
            m_MaterialPreviewSettings.OnHdriChanged -= OnHdriEnvironmentChanged;
            
            m_MaterialInspectorView = null;
            m_MaterialMapPreview = null;
            m_MaterialPreviewSelector = null;
        }

        public void SetMaterial(Material material)
        {
            m_Material = material;
            
            m_MaterialMapPreview?.SetMaterial(m_Material);
            m_MaterialPreviewSelector?.SetMaterial(m_Material);
            m_MaterialInspectorView?.SetMaterial(m_Material);
        }
        
        void OnMaterialMaterialPreviewSelected(MaterialPreviewItem item)
        {
            m_SelectedItem = item;
            m_MaterialMapPreview.SetSelectedPreviewItem(m_SelectedItem);
        }
        
        private void OnMaterialPropertiesChanged()
        {
            m_MaterialMapPreview.RefreshRender();
        }
        
        private void OnMaterialMapPreviewSelected(PrimitiveObjectTypes item)
        {
            m_MaterialMapPreview.SetPreviewType(item);
        }
        
        private void OnHdriEnvironmentChanged(HdriEnvironment environment)
        {
           m_MaterialMapPreview.SetHdriEnvironment(environment); 
        }
    }
}