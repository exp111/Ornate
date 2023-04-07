using System;

namespace Ornate.Lite.Messages.WS
{
    [AttributeUsage(AttributeTargets.Class)]
    public class WSMessageDataAttribute : Attribute
    {
        public string Method { get; set; }
        public WSMessageDataAttribute(string method)
        {
            Method = method;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class WSResponseDataAttribute : Attribute
    {
        public string RequestMethod { get; set; }
        public WSResponseDataAttribute(string requestMethod)
        {
            RequestMethod = requestMethod;
        }
    }
}
