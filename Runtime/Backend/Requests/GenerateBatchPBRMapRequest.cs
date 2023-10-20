using System;

namespace Unity.Muse.Common
{
    [Serializable]
    internal sealed class GenerateBatchPBRMapRequest : GuidItemRequest
    {
        public string[] map_types;

        public GenerateBatchPBRMapRequest(string guid, string[] map_types, string accessToken) : base(guid, accessToken)
        {
            this.map_types = map_types;
        }
    }
}
