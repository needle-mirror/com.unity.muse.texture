using System;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;
using Button = Unity.AppUI.UI.Button;

namespace Unity.Muse.Texture
{
    internal class MaterialPreviewSelector : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<MaterialPreviewSelector, UxmlTraits>
        {
        }

        private Button m_ArtifactPreviewButton;
        private Button m_MaterialPreviewButton;

        private IconButton m_DiffuseMapPreviewButton;
        private IconButton m_HeightMapPreviewButton;
        private IconButton m_MetallicMapPreviewButton;
        private IconButton m_SmoothnessMapPreviewButton;
        private IconButton m_AOMapPreviewButton;

        private Material m_Material;
        
        private const string k_SelectedClassName = "muse-material-inspector-preview--selected";
        private const string k_MainButtonSelectedClassName = "muse-material-inspector-preview-main--selected";

        private const string k_MapImageElementName = "map-image";
        
        public MaterialPreviewItem SelectedPreviewItem { get; private set; } = MaterialPreviewItem.Material;

        public event Action<MaterialPreviewItem> OnPreviewSelected;

        public MaterialPreviewSelector()
        {
            GenerateVisualTree(); 
            
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
        }

        private void GenerateVisualTree()
        {
            styleSheets.Add(Resources.Load<StyleSheet>("MaterialInspector"));
            
            name = "PreviewSelector";
            
            AddToClassList("muse-material-preview--selector");
            AddToClassList("muse-material--container-horizontal");
            
            var actionGroup = new ActionGroup()
            {
                compact = true,
                justified = false
            };
            
            actionGroup.Add(new Button()
            {
                name = "ArtifactPreview",
                tooltip = "Artifact Preview",
                trailingIcon = "grid-four--regular"
            }); 
            
            actionGroup.Add(new Button()
            {
                name = "MaterialPreview",
                tooltip = "Material Preview",
                trailingIcon = "generic-sphere--regular"
            });
            
            Add(actionGroup);
            
            Add(new IconButton()
            {
                name = "UnlitMap",
                tooltip = "Diffuse map preview"
            });
            
            Add(new IconButton()
            {
                name = "HeightMap",
                tooltip = "Height map preview"
            });
            
            Add(new IconButton()
            {
                name = "MetallicMap",
                tooltip = "Metallic map preview"
            });
            
            Add(new IconButton()
            {
                name = "SmoothnessMap",
                tooltip = "Smoothness map preview"
            });
            
            Add(new IconButton()
            {
                name = "AOMap",
                tooltip = "Ambient Occlusion map preview"
            });
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
            
            m_ArtifactPreviewButton = this.Q<Button>("ArtifactPreview");
            m_MaterialPreviewButton = this.Q<Button>("MaterialPreview");
            m_DiffuseMapPreviewButton = this.Q<IconButton>("UnlitMap");
            m_HeightMapPreviewButton = this.Q<IconButton>("HeightMap");
            m_MetallicMapPreviewButton = this.Q<IconButton>("MetallicMap");
            m_SmoothnessMapPreviewButton = this.Q<IconButton>("SmoothnessMap");
            m_AOMapPreviewButton = this.Q<IconButton>("AOMap");

            m_ArtifactPreviewButton.clickable.clicked += OnArtifactPreviewClicked;
            m_MaterialPreviewButton.clickable.clicked += OnMaterialPreviewClicked;
            m_DiffuseMapPreviewButton.clickable.clicked += OnDiffusePreviewClicked;
            m_HeightMapPreviewButton.clickable.clicked += OnHeightPreviewClicked;
            m_MetallicMapPreviewButton.clickable.clicked += OnMetallicPreviewClicked;
            m_SmoothnessMapPreviewButton.clickable.clicked += OnSmoothnessPreviewClicked;
            m_AOMapPreviewButton.clickable.clicked += OnAOPreviewClicked;

            InitializeIcons();
            UpdateSelectedState(SelectedPreviewItem);
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            UnregisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
            
            m_ArtifactPreviewButton.clickable.clicked -= OnArtifactPreviewClicked;
            m_MaterialPreviewButton.clickable.clicked -= OnMaterialPreviewClicked;
            m_DiffuseMapPreviewButton.clickable.clicked -= OnDiffusePreviewClicked;
            m_HeightMapPreviewButton.clickable.clicked -= OnHeightPreviewClicked;
            m_MetallicMapPreviewButton.clickable.clicked -= OnMetallicPreviewClicked;
            m_SmoothnessMapPreviewButton.clickable.clicked -= OnSmoothnessPreviewClicked;
            m_AOMapPreviewButton.clickable.clicked += OnAOPreviewClicked;
            
            m_ArtifactPreviewButton = null;
            m_MaterialPreviewButton = null;
            m_DiffuseMapPreviewButton = null;
            m_HeightMapPreviewButton = null;
            m_MetallicMapPreviewButton = null;
            m_SmoothnessMapPreviewButton = null;
            m_AOMapPreviewButton = null;
        }

        public void SelectItem(MaterialPreviewItem selectedItem, bool notify = true)
        {
            SelectedPreviewItem = selectedItem;
            UpdateSelectedState(selectedItem);
            
            if(notify)
                OnPreviewSelected?.Invoke(SelectedPreviewItem);
        }

        public void SetMaterial(Material material)
        {
            m_Material = material;
            InitializeIcons();
        }

        void InitializeIcons()
        {
            InitializeIcon(m_DiffuseMapPreviewButton, MuseMaterialProperties.baseMapKey);
            InitializeIcon(m_HeightMapPreviewButton, MuseMaterialProperties.heightMapKey);
            InitializeIcon(m_MetallicMapPreviewButton, MuseMaterialProperties.metallicMapKey);
            InitializeIcon(m_SmoothnessMapPreviewButton, MuseMaterialProperties.smoothnessMapKey);
            InitializeIcon(m_AOMapPreviewButton, MuseMaterialProperties.ambientOcclusionMapKey);
        }

        void InitializeIcon(IconButton button, int mapId)
        {
            if (button == null)
                return;

            if (m_Material == null)
            {
                button.style.display = DisplayStyle.None;
                return;
            }
            
            button.style.display = DisplayStyle.Flex;
            
            button.style.overflow = Overflow.Hidden;
            
            var container = button.Q("appui-button__leadingcontainer");
            if (container == null) return;
            
            container.RemoveFromClassList("unity-hidden");
            var imageContainer = container.Q<Image>();
            if (imageContainer == null) return;
            
            imageContainer.style.width = Length.Percent(100);
            imageContainer.style.height = Length.Percent(100);
            imageContainer.image = m_Material.GetTexture(mapId);
        }

        private void OnArtifactPreviewClicked()
        {
            SelectItem(MaterialPreviewItem.Artifact);
        }

        private void OnMaterialPreviewClicked()
        {
            SelectItem(MaterialPreviewItem.Material);
        }

        private void OnDiffusePreviewClicked()
        {
            SelectItem(MaterialPreviewItem.BaseMap);
        }

        private void OnHeightPreviewClicked()
        {
            SelectItem(MaterialPreviewItem.HeightMap);
        }

        private void OnMetallicPreviewClicked()
        {
            SelectItem(MaterialPreviewItem.MetallicMap);
        }

        private void OnSmoothnessPreviewClicked()
        {
            SelectItem(MaterialPreviewItem.SmoothnessMap);
        }
        
        private void OnAOPreviewClicked()
        {
             SelectItem(MaterialPreviewItem.AOMap);
        }
        
        void UpdateSelectedState(MaterialPreviewItem selectedItem)
        {
            m_MaterialPreviewButton.EnableInClassList(k_MainButtonSelectedClassName, selectedItem == MaterialPreviewItem.Material);
            m_ArtifactPreviewButton.EnableInClassList(k_MainButtonSelectedClassName, selectedItem == MaterialPreviewItem.Artifact);
            m_DiffuseMapPreviewButton.EnableInClassList(k_SelectedClassName, selectedItem == MaterialPreviewItem.BaseMap); 
            m_HeightMapPreviewButton.EnableInClassList(k_SelectedClassName, selectedItem == MaterialPreviewItem.HeightMap);
            m_MetallicMapPreviewButton.EnableInClassList(k_SelectedClassName, selectedItem == MaterialPreviewItem.MetallicMap);
            m_SmoothnessMapPreviewButton.EnableInClassList(k_SelectedClassName, selectedItem == MaterialPreviewItem.SmoothnessMap);
            m_AOMapPreviewButton.EnableInClassList(k_SelectedClassName, selectedItem == MaterialPreviewItem.AOMap);
        }
    }
}