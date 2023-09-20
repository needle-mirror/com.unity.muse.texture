using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Muse.Common;
using UnityEngine;

namespace Unity.Muse.Texture
{
    class UIMode : IUIMode
    {
        public const string modeKey = "TextToImage";
#if !UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#else
        [UnityEditor.InitializeOnLoadMethod]
#endif
        public static void RegisterUIMode()
        {
            UIModeFactory.RegisterUIMode<UIMode>(modeKey);
        }

        Model m_Model;
        public void Activate(MainUI mainUI)
        {
            m_Model = mainUI.model;
        }

        public void Deactivate()
        {
        }
    }
}
