using Unity.AppUI.UI;
using Unity.Muse.Common;
using UnityEngine.UIElements;
using Toggle = Unity.AppUI.UI.Toggle;

namespace Unity.Muse.Texture
{
    public class PaintingManipulatorSettings
    {
        VisualElement m_Root;
        PaintingManipulator m_PaintingManipulator;
        bool m_IsInitialized;

        private PaintingManipulatorSettings() { }

        public PaintingManipulatorSettings(PaintingManipulator paintingManipulator )
        {
            m_PaintingManipulator = paintingManipulator;
            Init();
        }

        void Init()
        {
            if (m_IsInitialized)
                return;

            m_Root = new VisualElement();
            m_Root.style.flexDirection = FlexDirection.Row;
            var radiusSlider = new TouchSliderFloat();
            radiusSlider.label = "Radius";
            radiusSlider.tooltip = "Adjust brush size";
            radiusSlider.incrementFactor = 0.1f;
            radiusSlider.formatString = "F1";
            radiusSlider.lowValue = 1.0f;
            radiusSlider.highValue = 10.0f;
            radiusSlider.value = m_PaintingManipulator.GetRadius();
            radiusSlider.style.width = 150.0f;

            radiusSlider.RegisterValueChangedCallback(evt =>
            {
                m_PaintingManipulator.SetRadius(evt.newValue);
            });

            var toggleErase = new Toggle {label = "Eraser", tooltip = "Toggle eraser mode"};
            toggleErase.RegisterValueChangedCallback(evt =>
            {
                m_PaintingManipulator.SetEraserMode(evt.newValue);
            });
            toggleErase.style.width = 100.0f;
            
            var clearButton = new ActionButton
            {
                name = "refiner-clear-button",
                label = "",
                tooltip = "Reset the mask",
                icon = "delete",
                quiet = true
            };
                
            clearButton.AddToClassList("muse-controltoolbar__actionbutton");
            clearButton.clicked += () =>
            {
                m_PaintingManipulator.ClearPainting();
            };
            m_Root.Add(toggleErase);
            m_Root.Add(radiusSlider);
            m_Root.Add(clearButton);
            m_IsInitialized = true;
        }

        public VisualElement GetSettings()
        {
            return m_Root;
        }
    }
}