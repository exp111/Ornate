using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Ornate.Lite.Messages.WS
{
    // battle: {"type": "monster:transient","uuid":"w__bq..."}
    [WSMessageData("battle")]
    public class BattleWSMessageData : BaseWSMessageData
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("uuid")]
        public string UUID { get; set; }
    }

    // {uuid:"a9ace...", "state_id":"1680", "turn":"<id>", "scene":"grass", "entity_uuid":"w__...", "battle_type":"monster:transient",...}
    [WSResponseData("battle")]
    public class BattleWSResponseData : BaseWSResponseData
    {
        [JsonPropertyName("uuid")]
        public string UUID { get; set; }

        [JsonPropertyName("state_id")]
        public string StateID { get; set; }

        [JsonPropertyName("turn")]
        public string Turn { get; set; }

        [JsonPropertyName("scene")]
        public string Scene { get; set; }

        [JsonPropertyName("entity_uuid")]
        public string EntityUUID { get; set; }

        [JsonPropertyName("battle_type")]
        public string BattleType { get; set; }
    }
}
