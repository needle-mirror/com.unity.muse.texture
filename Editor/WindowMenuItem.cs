using Unity.Muse.Common;
using Unity.Muse.Common.Editor;
using UnityEditor;

namespace Unity.Muse.Texture.Editor
{
    public static class WindowMenuItem
    {
        const string k_MuseTextureModeKey = "TextToImage";

        [MenuItem("Muse/New Texture Generator", false, 100)]
        public static void CreateTextureWindow()
        {
            EditorModelAssetEditor.OpenWindowForMode(k_MuseTextureModeKey);
        }
        
        [MenuItem("Muse/New Texture Generator", true)]
        public static bool ValidateCreateSpriteWindow()
        {
            return ModesFactory.GetModeIndexFromKey(k_MuseTextureModeKey) > -1;
        }
    }
}
