using System;

namespace Unity.Muse.Common
{
    [Serializable]
    internal sealed class GeneratePBRMapRequest : GuidItemRequest
    {
        public string map_type;

        public GeneratePBRMapRequest(string guid, string map_type, string accessToken) : base(guid, accessToken)
        {
            this.map_type = map_type;
        }
    }
}
