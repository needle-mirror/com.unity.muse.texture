using System;
using Unity.Muse.Common;

namespace Unity.Muse.Texture
{
    [Serializable]
    class UpscaleImageRequest : GuidItemRequest
    {
        public UpscaleImageRequest(string guid, string accessToken) : base(guid, accessToken)
        {
        }
    }
}
