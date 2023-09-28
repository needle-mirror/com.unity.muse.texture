using System;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Muse.Texture
{
    public class FactorSliderFloat : TouchSliderFloat
    {
        private LocalizedTextElement m_ValueLabel;

        public FactorSliderFloat() : base()
        {
            m_ValueLabel = this.Q<LocalizedTextElement>(classes: valueUssClassName);
            this.RegisterValueChangingCallback(OnValueLabelChanging);
            UpdateLabel();
        }

        private void OnValueLabelChanging(ChangingEvent<float> evt)
        {
           UpdateLabel(); 
        }
        
        private void UpdateLabel()
        {
            if(m_ValueLabel == null)
                return;
            
            m_ValueLabel.text = FormatFloat(value) + "x";
        }

        public override void SetValueWithoutNotify(float newValue)
        {
            base.SetValueWithoutNotify(newValue);
            UpdateLabel();
        }
        
        private static string FormatFloat(float value)
        {
            var roundedValue = (float)Math.Round(value, 2);

            return roundedValue.ToString(Mathf.Approximately(Mathf.Floor(roundedValue), roundedValue) ? "0" : "0.00");
        }
    }
}