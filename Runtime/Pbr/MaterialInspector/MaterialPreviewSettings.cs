using System;
using System.Collections.Generic;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Texture
{
    internal class MaterialPreviewSettings : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<MaterialPreviewSettings, UxmlTraits>
        {
        }

        private Dropdown m_PrimitivesDropdown;
        private Dropdown m_HdriDropdown;
        
        public event Action<PrimitiveObjectTypes> OnTargetPrimitiveChanged;
        internal event Action<HdriEnvironment> OnHdriChanged;
        
        private List<string> m_PrimitivesDropdownSrc = new()
        {
            "Sphere",
            "Cube",
            "Plane",
            "Cylinder",
            //"Custom"
        };

        private List<string> m_HdriDropdownSrc = new()
        {
            "Default Environment",
            "Outside Neutral",
            "Inside",
            "Day Outside",
            "Night Outside"
        };

        public MaterialPreviewSettings()
        {
            GenerateVisualTree();
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        private void GenerateVisualTree()
        {
            styleSheets.Add(Resources.Load<StyleSheet>("MaterialInspector"));
            
            name = "PreviewSettings";
            AddToClassList("muse-material--container-horizontal");
            AddToClassList("muse-material-preview--settings");
            
            var primitive = new Dropdown()
            {
                name= "PrimitivesSelector",
                tooltip = "Primitive Selection",
                
            };
            
            primitive.AddToClassList("muse-preview--primitives");
            
            Add(primitive);
            
            var hdri = new Dropdown()
            {
                name= "HdriSelector",
                tooltip = "Reflection Probe Selection",
            };
            
            hdri.AddToClassList("muse-preview--hdri");
            
            Add(hdri);
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            m_PrimitivesDropdown = this.Q<Dropdown>("PrimitivesSelector");
            m_HdriDropdown = this.Q<Dropdown>("HdriSelector");

            m_PrimitivesDropdown.RegisterValueChangedCallback(OnPrimitiveSelected);
            m_HdriDropdown.RegisterValueChangedCallback(OnHdriSelected);

            InitializeDropDowns();
        }
        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            m_PrimitivesDropdown.UnregisterValueChangedCallback(OnPrimitiveSelected);
            m_HdriDropdown.UnregisterValueChangedCallback(OnHdriSelected);
            
            m_PrimitivesDropdown = null;
            m_HdriDropdown = null;
        }

        private void InitializeDropDowns()
        {
            m_PrimitivesDropdown.bindItem = (item, i) => item.label = m_PrimitivesDropdownSrc[i];
            m_PrimitivesDropdown.sourceItems = m_PrimitivesDropdownSrc;
            m_PrimitivesDropdown.SetValueWithoutNotify(0);
            
            m_HdriDropdown.bindItem = (item, i) => item.label = m_HdriDropdownSrc[i];
            m_HdriDropdown.sourceItems = m_HdriDropdownSrc;
            m_HdriDropdown.SetValueWithoutNotify(0);
        }
        
        private void OnPrimitiveSelected(ChangeEvent<int> evt)
        {
            OnTargetPrimitiveChanged?.Invoke(evt.newValue switch
            {
                0 => PrimitiveObjectTypes.Sphere,
                1 => PrimitiveObjectTypes.Cube,
                2 => PrimitiveObjectTypes.Plane,
                3 => PrimitiveObjectTypes.Cylinder,
                4 => PrimitiveObjectTypes.Custom,
                _ => throw new ArgumentOutOfRangeException()
            });
        }
        
        
        private void OnHdriSelected(ChangeEvent<int> evt)
        {
            OnHdriChanged?.Invoke(evt.newValue switch
            {
                0 => HdriEnvironment.Default,
                1 => HdriEnvironment.OutsideNeutral,
                2 => HdriEnvironment.Inside,
                3 => HdriEnvironment.DayOutside,
                4 => HdriEnvironment.NightOutside,
                _ => throw new ArgumentOutOfRangeException()
            });
        }

        public void SelectPrimitive(PrimitiveObjectTypes type)
        {
            m_PrimitivesDropdown?.SetValueWithoutNotify((int)type);
        }
        
        internal void SelectHdri(HdriEnvironment environment)
        {
            m_HdriDropdown?.SetValueWithoutNotify((int)environment);
        }


        public void SetMaterial(Material mMaterial)
        {
        }
    }
}