using System;
using Unity.Muse.Common;

namespace Unity.Muse.Common
{
    [Serializable]
    public class DiffuseMapRequest: GuidItemRequest
    {

        /// <summary>
        /// Settings are not used but required for the API
        /// </summary>
        public DiffuseMapSettings settings;
        public DiffuseMapRequest(string guid, string accessToken) : base(guid, accessToken)
        {
        }

        [Serializable]
        public class DiffuseMapSettings
        {
            public int height;
            public int width;
        }
    }
}