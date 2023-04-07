using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Ornate.Lite.Messages.WS
{
    public interface IWSMessage
    {

    }

    // {"method":"ping","scope":"ping}
    // {"method":"getstate","data":{},"scope":"getstate","sid":"<sid>"}
    public class BaseWSMessage : IWSMessage
    {
        [JsonPropertyName("method")]
        public string Method { get; set; }
        [JsonPropertyName("scope")]
        public string Scope { get; set; }
        [JsonPropertyName("sid")] //TODO: some calls dont need data or auth
        public string? SID { get; set; }
        [JsonPropertyName("data")]
        public object? Data { get; set; }
    }

    public class BaseWSMessageData
    {

    }

    // {"success":true,"scope":"ping"}
    // {"success":true,"scope":"<id>","result":{...}}
    public class BaseWSResponse : IWSMessage
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
        [JsonPropertyName("scope")]
        public string Scope { get; set; }
        [JsonPropertyName("result")]
        public object? Result { get; set; }
    }

    public class BaseWSResponseData
    {

    }
}
