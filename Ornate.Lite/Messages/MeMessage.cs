using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Ornate.Lite.Messages
{
    // GET /api/me/?w=<width?>&v=<version>&x=<timestamp>&lang=en
    [Request("/api/me/")]
    public class MeRequest : BaseRequest
    {
        [JsonPropertyName("w")]
        public int Width { get; set; }
        [JsonPropertyName("v")]
        public string Version { get; set; }
    }

    [Response("/api/me/")]
    public class MeResponse : BaseResponse
    {
        [JsonPropertyName("required_version")]
        public string RequiredVersion { get; set; }
        [JsonPropertyName("recommended_version")]
        public string RecommendedVersion { get; set; }
        [JsonPropertyName("user")]
        public JsonObject User { get; set; }
        [JsonPropertyName("server")]
        public string Server { get; set; }
        [JsonPropertyName("online")]
        public bool Online { get; set; }
    }
}
