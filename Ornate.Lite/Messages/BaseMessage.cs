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
}
