using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ornate.Lite.Messages
{
    public enum RequestMethod
    {
        GET,
        POST,
    }

    // Defines that a class describes a request
    [AttributeUsage(AttributeTargets.Class)]
    public class RequestAttribute : Attribute
    {
        public string Path { get; set; }
        public RequestMethod Method { get; set; }
        public RequestAttribute(string path, RequestMethod method = RequestMethod.GET)
        {
            Path = path;
            Method = method;
        }
    }

    // Defines that a property should always be a query parameter
    [AttributeUsage(AttributeTargets.Property)]
    public class ForceParameterAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ResponseAttribute : Attribute
    {
        public string Path { get; set; }
        public ResponseAttribute(string path)
        {
            Path = path;
        }
    }

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
