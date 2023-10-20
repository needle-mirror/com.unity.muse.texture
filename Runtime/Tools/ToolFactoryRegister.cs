using Unity.Muse.Common;
using UnityEngine;

namespace Unity.Muse.Texture.Tools
{
    internal static class ToolFactoryRegister
    {
#if !UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        public static void RegisterAvailableTools()
        {
            AvailableToolsFactory.RegisterTool<PaintCanvasTool>(UIMode.modeKey);
        }
    }
}
