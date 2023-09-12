using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Muse.Common;
using UnityEngine;

namespace Unity.Muse.Texture
{
    class UIMode : IUIMode
    {
#if !UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#else
        [UnityEditor.InitializeOnLoadMethod]
#endif
        public static void RegisterUIMode()
        {
            UIModeFactory.RegisterUIMode<UIMode>("TextToImage");
        }

        Model m_Model;
        public void Activate(MainUI mainUI)
        {
            m_Model = mainUI.model;
            m_Model.OnSetOperatorDefaults += OnSetOperatorDefault;
        }

        public void Deactivate()
        {
            m_Model.OnSetOperatorDefaults -= OnSetOperatorDefault;
        }

        IEnumerable<IOperator> OnSetOperatorDefault(IEnumerable<IOperator> currentOperators)
        {
            // Remove these operator when going in refinement
            return currentOperators;
        }
    }
}
