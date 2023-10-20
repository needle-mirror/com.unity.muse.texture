using System;
using Unity.Muse.Common;

namespace Unity.Muse.Common
{
    [Serializable]
    internal class DiffuseMapRequest: GuidItemRequest
    {

        /// <summary>
        /// Settings are not used but required for the API
        /// </summary>
        public DiffuseMapSettings settings;
        public DiffuseMapRequest(string guid, string accessToken) : base(guid, accessToken)
        {
        }

        [Serializable]
        internal class DiffuseMapSettings
        {
            public int height;
            public int width;
        }
    }
}