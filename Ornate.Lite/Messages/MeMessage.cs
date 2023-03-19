using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Ornate.Lite.Messages
{
    [Request("/api/me/")]
    public class MeMessage // GET /api/me/?w=<width?>&v=<version>&x=<timestamp>&lang=en
    {
        //TODO: timestamp and language code are on (almost?) every message
        //      exception: during POSTs timestamp in url, lang (and other params) in POST

        [JsonPropertyName("w")]
        public int Width { get; set; }
        [JsonPropertyName("v")]
        public string Version { get; set; }
        [JsonPropertyName("x")]
        [ForceParameter]
        public long Timestamp { get; set; }
        [JsonPropertyName("lang")]
        public string Language { get; set; }
    }

    [Response("/api/me/")]
    public class MeResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
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
