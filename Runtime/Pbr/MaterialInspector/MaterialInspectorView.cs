using System;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;
using FloatField = Unity.AppUI.UI.FloatField;
using Toggle = Unity.AppUI.UI.Toggle;
using Vector2Field = Unity.AppUI.UI.Vector2Field;

namespace Unity.Muse.Texture
{
    internal class MaterialInspectorView : ScrollView
    {
        public new class UxmlFactory : UxmlFactory<MaterialInspectorView, UxmlTraits>
        {
        }

        private MaterialPreviewItem m_SelectedItem;
        private Material m_Material;

        private bool m_AttachedToPanel;
        
        private Vector2Field m_TilingField;
        private Vector2Field m_OffsetField;
        private FloatField m_RotationField;
        private Toggle m_FlipVerticalField;
        private Toggle m_FlipHorizontalField;
        private Toggle m_UseDisplacement;

        private TouchSliderFloat m_HeightIntensityField;
        private TouchSliderFloat m_MetallicIntensityField;
        private TouchSliderFloat m_RoughnessIntensityField;
        
        public event Action OnMaterialPropertiesChanged;

        public MaterialInspectorView()
        {
            GenerateVisualTree();
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        void GenerateVisualTree()
        {
            styleSheets.Add(Resources.Load<StyleSheet>("MaterialInspector"));
            
            name="MaterialInspectorView";
            AddToClassList("muse-material-inspector--view");
            
            GenerateEditSection();
            GenerateMapsSection();

        }

        void GenerateEditSection()
        {
            var editSection = new ExVisualElement();
            editSection.AddToClassList("muse-material--section");
            Add(editSection);

            var previewTitle = new Text()
            {
                text = "Preview"
            };
            
            previewTitle.AddToClassList("muse-inspector__title");
            
            editSection.Add(previewTitle);

            var tilingContainer = new ExVisualElement()
            {
                tooltip = "Tiling the maps"
            };
            tilingContainer.AddToClassList("muse-label--component");
            
            editSection.Add(tilingContainer);
            
            tilingContainer.Add(new Text()
            {
                text = "Tiling"
            });
            
            tilingContainer.Add(new Vector2Field()
            {
                name = "TilingField",
                size = Size.M
            }); 
            
            var offsetContainer = new ExVisualElement()
            {
                tooltip = "Offset the maps"
            };
            offsetContainer.AddToClassList("muse-label--component");
            editSection.Add(offsetContainer);
            
            offsetContainer.Add(new Text()
            {
                text = "Offset"
            });
            
            offsetContainer.Add(new Vector2Field()
            {
               name = "OffsetField",
               size = Size.M
            });
            
            var rotationContainer = new ExVisualElement()
            {
                tooltip = "Rotate the maps"
            };
            rotationContainer.AddToClassList("muse-label--component");
            editSection.Add(rotationContainer);
            
            rotationContainer.Add(new Text()
            {
                text = "Rotation"
            });
            
            rotationContainer.Add(new FloatField()
            {
                name = "RotationField",
                size = Size.M,
                unit = "DEG"
            });

            var verticalFlipContainer = new ExVisualElement()
            {
                tooltip = "Flip the maps vertically"
            };
            verticalFlipContainer.AddToClassList("muse-label--component-toggle");
            editSection.Add(verticalFlipContainer);
            
            verticalFlipContainer.Add(new Text()
            {
                text = "Flip Vertical"
            });
            
            verticalFlipContainer.Add(new Toggle()
            {
                name = "VerticalFlipField"
            });
            
            var horizontalFlipContainer = new ExVisualElement()
            {
                tooltip = "Flip the maps horizontally"
            };
            horizontalFlipContainer.AddToClassList("muse-label--component-toggle");
            editSection.Add(horizontalFlipContainer);
            
            horizontalFlipContainer.Add(new Text()
            {
                text = "Flip Horizontal"
            });
            
            horizontalFlipContainer.Add(new Toggle()
            {
                name = "HorizontalFlipField"
            });
            
            var useDisplacementContainer = new ExVisualElement()
            {
                tooltip = "Use displacement"
            };
            useDisplacementContainer.AddToClassList("muse-label--component-toggle");
            editSection.Add(useDisplacementContainer);
            
            useDisplacementContainer.Add(new Text()
            {
                text = "Use Displacement"
            });
            
            useDisplacementContainer.Add(new Toggle()
            {
                name = "UseDisplacementField"
            });
        }
        
        private void GenerateMapsSection()
        {
            var mapsSection = new ExVisualElement();
            mapsSection.AddToClassList("muse-material--section");
            Add(mapsSection);
            
            var mapsTitle = new Text()
            {
                text = "Maps"
            };
            
            mapsTitle.AddToClassList("muse-inspector__title");
            
            mapsSection.Add(mapsTitle);
            
            mapsSection.Add(new FactorSliderFloat()
            {
                name = "HeightIntensityField",
                label = "Height",
                tooltip = "Height",
                lowValue = -20f,
                highValue = 20f,
                value = 0f,
                size = Size.M
            });
            
            mapsSection.Add(new FactorSliderFloat()
            {
                name = "MetallicIntensityField",
                label = "Metallic",
                tooltip = "Metallic intensity",
                lowValue = 0f,
                highValue = 20f,
                value = 1f,
                size = Size.M
            });
            
            mapsSection.Add(new FactorSliderFloat()
            {
                name = "RoughnessIntensityField",
                label = "Roughness",
                tooltip = "Roughness intensity",
                lowValue = 0f,
                highValue = 3f,
                value = 1f,
                size = Size.M
            });
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            m_AttachedToPanel = true;
            
            m_TilingField = this.Q<Vector2Field>("TilingField");
            m_OffsetField = this.Q<Vector2Field>("OffsetField");
            m_RotationField = this.Q<FloatField>("RotationField");
            m_FlipVerticalField = this.Q<Toggle>("VerticalFlipField");
            m_FlipHorizontalField = this.Q<Toggle>("HorizontalFlipField");
            m_UseDisplacement = this.Q<Toggle>("UseDisplacementField");
            m_HeightIntensityField = this.Q<TouchSliderFloat>("HeightIntensityField");
            m_MetallicIntensityField = this.Q<TouchSliderFloat>("MetallicIntensityField");
            m_RoughnessIntensityField = this.Q<TouchSliderFloat>("RoughnessIntensityField");
            
            InitializePropertiesValues();
            RegisterPropertyChangeCallbacks();
        }
        
        private void InitializePropertiesValues()
        {
            if (m_Material == null || !m_AttachedToPanel) 
                return;

            m_TilingField.SetValueWithoutNotify(m_Material.GetVector(MuseMaterialProperties.tilingKey));
            m_OffsetField.SetValueWithoutNotify(m_Material.GetVector(MuseMaterialProperties.offsetKey));
            m_RotationField.SetValueWithoutNotify(m_Material.GetFloat(MuseMaterialProperties.rotationKey));
            m_FlipVerticalField.SetValueWithoutNotify(m_Material.GetFloat(MuseMaterialProperties.flipVertical) > 0.0);
            m_FlipHorizontalField.SetValueWithoutNotify(m_Material.GetFloat(MuseMaterialProperties.flipHorizontal) > 0.0);
            m_UseDisplacement.SetValueWithoutNotify(m_Material.GetFloat(MuseMaterialProperties.useDisplacement) > 0.0);
            m_HeightIntensityField.SetValueWithoutNotify(m_Material.GetFloat(MuseMaterialProperties.heightIntensity));
            m_MetallicIntensityField.SetValueWithoutNotify(m_Material.GetFloat(MuseMaterialProperties.metallicIntensity));
            m_RoughnessIntensityField.SetValueWithoutNotify(m_Material.GetFloat(MuseMaterialProperties.roughnessIntensity));
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            m_AttachedToPanel = false;
            
            UnRegisterPropertyChangeCallbacks();

            m_TilingField = null; 
            m_OffsetField = null; 
            m_RotationField = null; 
            m_FlipVerticalField = null; 
            m_FlipHorizontalField = null; 
            m_UseDisplacement = null;
            m_HeightIntensityField = null; 
            m_MetallicIntensityField = null; 
            m_RoughnessIntensityField = null; 
        }

        private void RegisterPropertyChangeCallbacks()
        {
            m_TilingField.RegisterValueChangingCallback(evt => MaterialPropertiesChanged());
            m_OffsetField.RegisterValueChangingCallback(evt => MaterialPropertiesChanged());
            m_RotationField.RegisterValueChangingCallback(evt => MaterialPropertiesChanged());
            m_FlipVerticalField.RegisterValueChangedCallback(evt => MaterialPropertiesChanged());
            m_FlipHorizontalField.RegisterValueChangedCallback(evt => MaterialPropertiesChanged());
            m_UseDisplacement.RegisterValueChangedCallback(evt => MaterialPropertiesChanged());
            m_HeightIntensityField.RegisterValueChangingCallback(evt => MaterialPropertiesChanged());
            m_HeightIntensityField.RegisterValueChangedCallback(evt => MaterialPropertiesChanged());
            m_MetallicIntensityField.RegisterValueChangingCallback(evt => MaterialPropertiesChanged());
            m_MetallicIntensityField.RegisterValueChangedCallback(evt => MaterialPropertiesChanged());
            m_RoughnessIntensityField.RegisterValueChangingCallback(evt => MaterialPropertiesChanged());
            m_RoughnessIntensityField.RegisterValueChangedCallback(evt => MaterialPropertiesChanged());
        }

        private void UnRegisterPropertyChangeCallbacks()
        {
            m_TilingField.UnregisterValueChangingCallback(evt => MaterialPropertiesChanged());
            m_OffsetField.UnregisterValueChangingCallback(evt => MaterialPropertiesChanged());
            m_RotationField.UnregisterValueChangingCallback(evt => MaterialPropertiesChanged());
            m_FlipVerticalField.UnregisterValueChangedCallback(evt => MaterialPropertiesChanged());
            m_FlipHorizontalField.UnregisterValueChangedCallback(evt => MaterialPropertiesChanged());
            m_UseDisplacement.UnregisterValueChangedCallback(evt => MaterialPropertiesChanged());
            m_HeightIntensityField.UnregisterValueChangingCallback(evt => MaterialPropertiesChanged());
            m_MetallicIntensityField.UnregisterValueChangingCallback(evt => MaterialPropertiesChanged());
            m_RoughnessIntensityField.UnregisterValueChangingCallback(evt => MaterialPropertiesChanged());
        }

        private void SetPropertiesValues()
        {
            if (m_Material == null || !m_AttachedToPanel)
                return;
            
            m_Material.SetVector(MuseMaterialProperties.tilingKey, m_TilingField.value);
            m_Material.SetVector(MuseMaterialProperties.offsetKey, m_OffsetField.value);
            m_Material.SetFloat(MuseMaterialProperties.rotationKey, m_RotationField.value);
            m_Material.SetFloat(MuseMaterialProperties.flipVertical, m_FlipVerticalField.value ? 1.0f : 0.0f);
            m_Material.SetFloat(MuseMaterialProperties.flipHorizontal, m_FlipHorizontalField.value ? 1.0f : 0.0f);
            m_Material.SetFloat(MuseMaterialProperties.useDisplacement, m_UseDisplacement.value ? 1.0f : 0.0f);
            m_Material.SetFloat(MuseMaterialProperties.heightIntensity, m_HeightIntensityField.value);
            m_Material.SetFloat(MuseMaterialProperties.metallicIntensity, m_MetallicIntensityField.value);
            m_Material.SetFloat(MuseMaterialProperties.roughnessIntensity, m_RoughnessIntensityField.value);
        }

        public void SetMaterial(Material material)
        {
            m_Material = material;
            InitializePropertiesValues();
        }

        public void SetSelectedPreviewItem(MaterialPreviewItem previewItem)
        {
            m_SelectedItem = previewItem;
        }

        void MaterialPropertiesChanged()
        {
            SetPropertiesValues();
            OnMaterialPropertiesChanged?.Invoke();
        }
    }
}