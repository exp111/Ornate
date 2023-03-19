using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Ornate.Lite.Messages
{
    [Request("/api/daily/", RequestMethod.POST)]
    public class DailyRequest
    {
        [JsonPropertyName("x")]
        [ForceParameter]
        public long Timestamp { get; set; }
        [JsonPropertyName("lang")]
        public string Language { get; set; }
    }
}
