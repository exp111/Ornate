using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Ornate.Lite.Messages
{
    public interface IRequest
    {

    }
    public class BaseRequest : IRequest
    {
        [JsonPropertyName("x")]
        [ForceParameter]
        public long Timestamp { get; set; }

        [JsonPropertyName("lang")]
        public string Language { get; set; }
    }

    public interface IResponse
    {

    }

    public class BaseResponse : IResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
    }

    public interface IWSMessage
    {

    }

    //TODO: seperate into BaseDataMessage and BaseMessage?
    // {"method":"ping","scope":"ping}
    // {"method":"getstate","data":{},"scope":"getstate","sid":"<sid>"}
    public class BaseMessage : IWSMessage
    {
        [JsonPropertyName("method")]
        public string Method { get; set; }
        [JsonPropertyName("scope")]
        public string Scope { get; set; }
        [JsonPropertyName("sid")]
        public string? SID { get; set; }
        [JsonPropertyName("data")]
        public JsonObject? Data { get; set; }
    }

    //TODO: seperate into BaseWSResponse and BaseWSResultResponse?
    // {"success":true,"scope":"ping"}
    // {"success":true,"scope":"<id>","result":{...}}
    public class BaseWSResponse : IWSMessage
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
        [JsonPropertyName("scope")]
        public string Scope { get; set; }
        [JsonPropertyName("result")]
        public JsonObject? Result { get; set; }
    }
}
